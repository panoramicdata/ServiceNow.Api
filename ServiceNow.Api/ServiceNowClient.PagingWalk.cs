using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ServiceNow.Api.Exceptions;

namespace ServiceNow.Api;

public partial class ServiceNowClient
{
	internal async Task<List<JObject>> GetAllByQueryInternalJObjectAsync(
		string tableName,
		string? query,
		List<string>? fieldList,
		string? extraQueryString,
		int pageSize,
		string? customOrderByField,
		CancellationToken cancellationToken)
	{
		var (orderByCommand, orderByField) = ResolveOrderBy(customOrderByField);

		_logger.LogTrace($"Entered {nameof(GetAllByQueryInternalJObjectAsync)}" +
						 $" type: {typeof(JObject)}, {DescribeQuery(tableName, query, fieldList, extraQueryString)}" +
						 $", {nameof(orderByField)}: {Describe(orderByField)}" +
						 $", PageSize: {pageSize}" +
						 ".");

		// Ordering is applied here, so a caller-supplied ORDERBY would conflict with the paging window
		if (query?.Contains("ORDERBY") == true)
		{
			throw new ServiceNowApiException("ORDERBY not supported in query due to paging mechanism");
		}

		query = query == null
			? $"{orderByCommand}{orderByField}"
			: query + $"^{orderByCommand}{orderByField}";

		var (items, apiReportedTotalCount) = await WalkPagesAsync(tableName, query, BuildPagingFieldList(fieldList, orderByField), extraQueryString, pageSize, orderByField, cancellationToken).ConfigureAwait(false);

		// The paging window re-reads boundary records, so de-duplicate on sys_id
		var unique = new Dictionary<string, JObject>();
		foreach (var jObject in items)
		{
			unique[jObject["sys_id"]?.ToString() ?? string.Empty] = jObject;
		}

		items = [.. unique.Values];

		_logger.LogTrace($"Initial reported TotalCount from API: {apiReportedTotalCount}");
		if (!ItemsReturnedInsideTolerance(items.Count, apiReportedTotalCount))
		{
			throw new Exception($"Expected {apiReportedTotalCount:N0} items but retrieved {items.Count:N0}, which is not within the {_options.ValidateCountItemsReturnedTolerance} tolerance.");
		}

		TrimToRequestedFields(items, fieldList);

		_logger.LogDebug($"Retrieved {items.Count:N0} items from ServiceNow.");
		return items;
	}

	private (string Command, string Field) ResolveOrderBy(string? customOrderByField)
	{
		var orderByField = string.IsNullOrWhiteSpace(customOrderByField)
			? _options.PagingFieldName
			: customOrderByField;

		return orderByField.StartsWith('-')
			? ("ORDERBYDESC", orderByField[1..])
			: ("ORDERBY", orderByField);
	}

	private static List<string> BuildPagingFieldList(List<string>? fieldList, string orderByField)
	{
		var actualFieldList = new List<string>(fieldList ?? []);

		// The paging field and sys_id (for de-duplication) must be fetched even when not requested
		if (actualFieldList.Count > 0)
		{
			foreach (var required in new[] { orderByField, "sys_id" })
			{
				if (!actualFieldList.Contains(required))
				{
					actualFieldList.Add(required);
				}
			}
		}

		return actualFieldList;
	}

	private async Task<(List<JObject> Items, int ApiReportedTotalCount)> WalkPagesAsync(
		string tableName,
		string query,
		List<string> actualFieldList,
		string? extraQueryString,
		int pageSize,
		string orderByField,
		CancellationToken cancellationToken)
	{
		var maxDateTimeRetrieved = DateTimeOffset.MinValue;
		var allItems = new List<JObject>();
		var apiReportedTotalCount = 0;
		var queryWithPagingOffset = query;
		var firstPage = true;
		while (true)
		{
			var response = await GetPageByQueryInternalAsync<JObject>(0, pageSize, tableName, queryWithPagingOffset, actualFieldList, extraQueryString, cancellationToken).ConfigureAwait(false);
			if (firstPage)
			{
				// Later pages carry a changed criteria, so only the first page reports the true total
				apiReportedTotalCount = response.TotalCount;
				firstPage = false;
			}

			var items = response.Items;
			allItems.AddRange(items);
			_logger.LogTrace($"Last request received {items.Count} items");

			// Only an empty page ends the walk: read ACLs apply after sysparm_limit, so a short page can still be followed by data
			if (items.Count == 0)
			{
				break;
			}

			var previousMaxDateTimeRetrieved = maxDateTimeRetrieved;

			if (items.All(item => item[orderByField] is null))
			{
				AssertCompleteWithoutPagingField(tableName, orderByField, items.Count == pageSize, allItems.Count, apiReportedTotalCount);
				break;
			}

			maxDateTimeRetrieved = items.Max(jObject => ParsePagingFieldValue(jObject, orderByField, tableName));

			if (previousMaxDateTimeRetrieved == maxDateTimeRetrieved)
			{
				AssertWindowCanStop(items.Count == pageSize);
				break;
			}

			queryWithPagingOffset = $"{query}^{orderByField}>={maxDateTimeRetrieved.UtcDateTime:yyyy-MM-dd HH:mm:ss}";
		}

		return (allItems, apiReportedTotalCount);
	}

	private void AssertCompleteWithoutPagingField(string tableName, string orderByField, bool pageWasFull, int retrieved, int apiReportedTotalCount)
	{
		// Without the paging field the window cannot advance, which only matters while records remain
		if (!pageWasFull && ItemsReturnedInsideTolerance(retrieved, apiReportedTotalCount))
		{
			return;
		}

		throw new ServiceNowApiException(
			$"The table / view '{tableName}' does not have the '{orderByField}' field, which is " +
			$"required to page through the full result set ({retrieved:N0} of " +
			$"{apiReportedTotalCount:N0} records retrieved). Set the {nameof(Options.PagingFieldName)} " +
			"option (or the customOrderByField parameter) to a date/time field that this table / view " +
			"does have, or request a single page with skip / take.");
	}

	private static void AssertWindowCanStop(bool pageWasFull)
	{
		// A full page sharing one timestamp needs a larger page; a short one is just the boundary record re-read by '>='
		if (pageWasFull)
		{
			throw new ServiceNowApiException("Paging window has not increased, try a larger page size.");
		}
	}

	private static void TrimToRequestedFields(List<JObject> items, List<string>? fieldList)
	{
		if (items.Count == 0 || fieldList is not { Count: > 0 })
		{
			return;
		}

		var actualPropertyNames = items[0].Properties().Select(p => p.Name).OrderBy(name => name).ToList();
		if (fieldList.OrderBy(name => name).SequenceEqual(actualPropertyNames))
		{
			return;
		}

		var propertiesToRemove = actualPropertyNames.Where(name => !fieldList.Contains(name)).ToList();
		foreach (var item in items)
		{
			foreach (var propertyName in propertiesToRemove)
			{
				_ = item.Remove(propertyName);
			}
		}
	}
}

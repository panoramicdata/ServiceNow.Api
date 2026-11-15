using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ServiceNow.Api.Exceptions;
using ServiceNow.Api.Tables;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;

namespace ServiceNow.Api;

public partial class ServiceNowClient
{
	public Task<List<T>> GetAllByQueryAsync<T>(string? query = null, CancellationToken cancellationToken = default) where T : Table
	{
		_logger.LogDebug($"Calling {nameof(GetAllByQueryAsync)} type: {typeof(T)}, {nameof(query)}:{query ?? "<not set>"}.");
		return GetAllByQueryInternalAsync<T>(Table.GetTableName<T>(), query, null, null, _options.PageSize, cancellationToken);
	}

	internal async Task<List<T>> GetAllByQueryInternalAsync<T>(
		string tableName,
		string? query,
		List<string>? fieldList,
		string? extraQueryString,
		int take,
		CancellationToken cancellationToken)
	{
		_logger.LogTrace($"Entered {nameof(GetAllByQueryInternalAsync)} type: {typeof(T)}, {nameof(tableName)}: {tableName}, {nameof(query)}: {query ?? "<not set>"}, {nameof(fieldList)}: {Describe(fieldList)}, {nameof(extraQueryString)}: {Describe(extraQueryString)}.");

		// Sort by something so paging does not return duplicates
		if (query?.Contains("ORDERBY") != true)
		{
			query = query == null
				? $"ORDERBY{_options.PagingFieldName}"
				: query + $"^ORDERBY{_options.PagingFieldName}";
		}

		var skip = 0;
		var finalResult = new Page<T>();
		while (true)
		{
			var response = await GetPageByQueryInternalAsync<T>(skip, take, tableName, query, fieldList, extraQueryString, cancellationToken).ConfigureAwait(false);
			skip += take;
			finalResult.Items.AddRange(response.Items);

			// A full page means there are probably more
			if (response.Items.Count == take)
			{
				continue;
			}

			finalResult.TotalCount = response.TotalCount;
			if (!ItemsReturnedInsideTolerance(finalResult.Items.Count, finalResult.TotalCount))
			{
				throw new Exception($"Expected {finalResult.TotalCount:N0} items but only retrieved {finalResult.Items.Count:N0}, which is not within the {_options.ValidateCountItemsReturnedTolerance} tolerance.");
			}

			return finalResult.Items;
		}
	}

	private bool ItemsReturnedInsideTolerance(int countItems, int totalExpected)
	{
		var message = $"Checking whether {countItems} of {totalExpected} expected is acceptable... ";
		bool isCountItemsOk;

		if (!_options.ValidateCountItemsReturned)
		{
			isCountItemsOk = true;
			message += $"Yes - {nameof(_options.ValidateCountItemsReturned)} is disabled.";
		}
		else
		{
			// Only a shortfall is enforced: the total is a snapshot from the first page, and a live table gains records during a long walk
			isCountItemsOk = countItems >= totalExpected - _options.ValidateCountItemsReturnedTolerance;
			message += $"{(isCountItemsOk ? "Yes - not short of" : "No - short of")} the expected total, allowing for a tolerance of {_options.ValidateCountItemsReturnedTolerance}.";
		}

		_logger.LogDebug(message);
		return isCountItemsOk;
	}

	/// <summary>
	/// Get data by query
	/// </summary>
	/// <param name="tableName">Table name</param>
	/// <param name="query">Query</param>
	/// <param name="fieldList">Fields to retrieve</param>
	/// <param name="extraQueryString">Extra query string</param>
	/// <param name="customOrderByField">Optional field to order by when paging; default=sys_created_on</param>
	/// <param name="pageSize">Optional page size for paging; default=1000</param>
	/// <param name="cancellationToken">CancellationToken</param>
	/// <returns></returns>
	public async Task<List<JObject>> GetAllByQueryAsync(
		string tableName,
		string? query = null,
		List<string>? fieldList = null,
		string? extraQueryString = null,
		string? customOrderByField = null,
		int? pageSize = null,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug($"Calling {nameof(GetAllByQueryAsync)}" +
						 $" {nameof(tableName)}: {tableName}" +
						 $", {nameof(query)}: {query ?? "<not set>"}" +
						 $", {nameof(fieldList)}: {Describe(fieldList)}" +
						 $", {nameof(extraQueryString)}: {Describe(extraQueryString)}" +
						 $", {nameof(customOrderByField)}: {Describe(customOrderByField)}" +
						 $", {nameof(pageSize)}: {(pageSize.HasValue ? pageSize.Value.ToString() : "<not set>")}" +
						 ".");

		// Has the user constrained by sysparm_limit?
		var limitMatches = Regex.Match(extraQueryString ?? string.Empty, "sysparm_limit=(\\d+)");
		if (limitMatches.Success && int.TryParse(limitMatches.Groups[1].Value, out var theInt))
		{
			var page = await GetPageByQueryInternalAsync<JObject>(0, theInt, tableName, query, fieldList, extraQueryString, cancellationToken).ConfigureAwait(false);
			return page.Items;
		}

		return await GetAllByQueryInternalJObjectAsync(
			tableName,
			query,
			fieldList,
			extraQueryString,
			pageSize ?? _options.PageSize,
			customOrderByField,
			cancellationToken
			).ConfigureAwait(false);
	}

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
						 $" type: {typeof(JObject)}" +
						 $", {nameof(tableName)}: {tableName}" +
						 $", {nameof(query)}: {query ?? "<not set>"}" +
						 $", {nameof(fieldList)}: {Describe(fieldList)}" +
						 $", {nameof(extraQueryString)}: {Describe(extraQueryString)}" +
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

	public Task<Page<JObject>> GetPageByQueryAsync(
		int skip,
		int take,
		string tableName,
		string? query = null,
		List<string>? fieldList = null,
		string? extraQueryString = null,
		CancellationToken cancellationToken = default)
		=> GetPageByQueryInternalAsync<JObject>(skip, take, tableName, query, fieldList, extraQueryString, cancellationToken);

	public Task<Page<T>> GetPageByQueryAsync<T>(
		int skip,
		int take,
		string? query = null,
		CancellationToken cancellationToken = default) where T : Table
		=> GetPageByQueryInternalAsync<T>(skip, take, Table.GetTableName<T>(), query, null, null, cancellationToken);

	private async Task<Page<T>> GetPageByQueryInternalAsync<T>(
		int skip,
		int take,
		string tableName,
		string? query,
		List<string>? fieldList,
		string? extraQueryString,
		CancellationToken cancellationToken)
	{
		_logger.LogTrace($"Entered {nameof(GetPageByQueryInternalAsync)}" +
						 $" type: {typeof(T)}" +
						 $", {nameof(tableName)}: {tableName}" +
						 $", {nameof(query)}: {query ?? "<not set>"}" +
						 $", {nameof(fieldList)}: {Describe(fieldList)}" +
						 $", {nameof(skip)}: {skip}" +
						 $", {nameof(take)}: {take}" +
						 ".");

		var subUrl = $"api/now/table/{tableName}" +
						$"?sysparm_offset={skip}" +
						$"&sysparm_limit={take}" +
						(!string.IsNullOrWhiteSpace(query) ? $"&sysparm_query={HttpUtility.UrlEncode(query)}" : null) +
						(fieldList?.Any() == true ? "&" : "") +
						BuildFieldListQueryParameter(fieldList) +
						(string.IsNullOrWhiteSpace(extraQueryString) ? "" : "&" + extraQueryString);
		var pageResult = await GetInternalAsync<Page<T>>(subUrl, cancellationToken).ConfigureAwait(false);
		if (!string.IsNullOrWhiteSpace(pageResult.Status))
		{
			var message = $"An status response of 'failure' was observed. Error Message: '{pageResult.Error?.Message}'. Error Detail: '{pageResult.Error?.Detail}'";
			throw new ServiceNowApiException(message);
		}

		return pageResult;
	}

	/// <summary>
	/// Reads the ordering field out of a returned row as a UTC DateTimeOffset, for use as the paging window boundary.
	/// </summary>
	/// <remarks>
	/// With sysparm_display_value=all every field is a { display_value, value } object, so the raw "value" is
	/// preferred. Text is parsed with the invariant culture so a display-formatted date reads the same on every host.
	/// </remarks>
	private static DateTimeOffset ParsePagingFieldValue(JObject jObject, string orderByField, string tableName)
	{
		var token = jObject[orderByField];

		if (token is JObject valueObject)
		{
			token = valueObject["value"] ?? valueObject["display_value"];
		}

		// Newtonsoft has already turned ISO-8601 text into a date; ToString() would render it in the host culture and time zone
		if (token?.Type == JTokenType.Date)
		{
			return token.ToObject<DateTimeOffset>().ToUniversalTime();
		}

		var text = token?.ToString();

		return !string.IsNullOrWhiteSpace(text)
			&& DateTimeOffset.TryParse(
				text,
				CultureInfo.InvariantCulture,
				DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
				out var parsed)
			? parsed
			: throw new ServiceNowApiException(
			$"Could not interpret the paging field '{orderByField}' on table '{tableName}' as a date and time. " +
			$"The value was '{text ?? "<null>"}'. Paging requires a date/time field, so either set the " +
			$"{nameof(Options.PagingFieldName)} option (or the customOrderByField parameter) to one, or use a paged query instead.");
	}
}

using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ServiceNow.Api.Tables;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ServiceNow.Api;

public partial class ServiceNowClient
{
	public Task<List<T>> GetAllByQueryAsync<T>([Optional] string? query, [Optional] CancellationToken cancellationToken) where T : Table
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
		_logger.LogTrace($"Entered {nameof(GetAllByQueryInternalAsync)} type: {typeof(T)}, {DescribeQuery(tableName, query, fieldList, extraQueryString)}.");

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
		[Optional] string? query,
		[Optional] List<string>? fieldList,
		[Optional] string? extraQueryString,
		[Optional] string? customOrderByField,
		[Optional] int? pageSize,
		[Optional] CancellationToken cancellationToken)
	{
		_logger.LogDebug($"Calling {nameof(GetAllByQueryAsync)} {DescribeQuery(tableName, query, fieldList, extraQueryString)}" +
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
}

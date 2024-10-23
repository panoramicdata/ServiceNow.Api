
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceNow.Api.Exceptions;
using ServiceNow.Api.MetaData;
using ServiceNow.Api.Tables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ServiceNow.Api;

public class ServiceNowClient : IServiceNowClient
{
	private readonly ILogger _logger;
	private readonly HttpClient _httpClient;
	private readonly Options _options;

	public ServiceNowClient(
		 string account,
		 string username,
		 string password,
		Options? options = null)
	{
		_options = options ?? new();

		// Accept the ILogger passed in on options or create a NullLogger
		_logger = _options.Logger ?? new NullLogger<ServiceNowClient>();

		AccountName = account;
		if (account == null)
		{
			throw new ArgumentNullException(nameof(account));
		}

		if (username == null)
		{
			throw new ArgumentNullException(nameof(username));
		}

		if (password == null)
		{
			throw new ArgumentNullException(nameof(password));
		}

		var baseAddress = _options.Environment switch
		{
			ServiceNowEnvironment.GCC => $"https://{account}.servicenowservices.com",
			_ => $"https://{account}.service-now.com",
		};
		var httpClientHandler = new HttpClientHandler();
		_httpClient = new HttpClient(httpClientHandler)
		{
			BaseAddress = new Uri(baseAddress),
			DefaultRequestHeaders =
			{
				Accept = {new MediaTypeWithQualityHeaderValue("application/json")},
			}
		};
		_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}")));
		_logger.LogDebug("Created ServiceNowClient instance.");
	}

	public ServiceNowClient(
		string account,
		string username,
		string password,
		ILogger? iLogger = null)
	: this(account, username, password, new Options { Logger = iLogger })
	{
	}

	public string AccountName { get; } = string.Empty;

	public void Dispose() => _httpClient?.Dispose();

	#region GetAllByQueryAsync<T>
	public Task<List<T>> GetAllByQueryAsync<T>() where T : Table
	=> GetAllByQueryAsync<T>(null, default);

	public Task<List<T>> GetAllByQueryAsync<T>(CancellationToken cancellationToken) where T : Table
		=> GetAllByQueryAsync<T>(null, cancellationToken);

	public Task<List<T>> GetAllByQueryAsync<T>(string query) where T : Table
		=> GetAllByQueryAsync<T>(query, default);

	public Task<List<T>> GetAllByQueryAsync<T>(string? query, CancellationToken cancellationToken) where T : Table
	{
		_logger.LogDebug($"Calling {nameof(GetAllByQueryAsync)}" +
						 $" type: {typeof(T)}" +
						 $", {nameof(query)}:{query ?? "<not set>"}" +
						 ".");
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
		_logger.LogTrace($"Entered {nameof(GetAllByQueryInternalAsync)}" +
						 $" type: {typeof(T)}" +
						 $", {nameof(tableName)}: {tableName}" +
						 $", {nameof(query)}: {query ?? "<not set>"}" +
						 $", {nameof(fieldList)}: {(fieldList?.Any() == true ? string.Join(", ", fieldList) : "<not set>")}" +
						 $", {nameof(extraQueryString)}: {(string.IsNullOrWhiteSpace(extraQueryString) ? "<not set>" : extraQueryString)}" +
						 ".");
		// To avoid issues with duplicates we should sort by something.
		// Does the query contain an ORDERBY?
		if (query?.Contains("ORDERBY") != true)
		{
			// NO - So set a default
			if (query == null)
			{
				query = $"ORDERBY{_options.PagingFieldName}";
			}
			else
			{
				query += $"^ORDERBY{_options.PagingFieldName}";
			}
		}

		var skip = 0;
		var finished = false;
		// Prepare our final response
		var finalResult = new Page<T>();
		// While the last request returned at least the pageSize then we continue
		// TODO - FIX PAGING! This should take the maximum item in the ORDERBY and use that as a ">=", eliminating any duplicates in the paging output
		while (!finished)
		{
			// Skip the number of "take" entries each time
			var response = await GetPageByQueryInternalAsync<T>(skip, take, tableName, query, fieldList, extraQueryString, cancellationToken).ConfigureAwait(false);
			// Get the next page next time round
			skip += take;
			// Add this response to the list
			finalResult.Items.AddRange(response.Items);
			// If we got at least the number we asked for then there are probably more
			if (response.Items.Count == take)
			{
				continue;
			}

			// All done
			finished = true;

			// If required, double check how many we got is how many we should have
			finalResult.TotalCount = response.TotalCount;
			if (!ItemsReturnedInsideTolerance(finalResult.Items.Count, finalResult.TotalCount))
			{
				throw new Exception($"Expected {finalResult.TotalCount} entries but only retrieved {finalResult.Items.Count} which is not within the {_options.ValidateCountItemsReturnedTolerance} tolerance");
			}
		}

		// https://community.servicenow.com/community?id=community_question&sys_id=bd7f8725dbdcdbc01dcaf3231f961949
		// See if we have any dupes based on sys_id
		if (finalResult.Items.Count > 0)
		{
			// Do we have a sys_id to work with

		}

		return finalResult.Items;
	} 
	#endregion

	private bool ItemsReturnedInsideTolerance(int countItems, int totalExpected)
	{
		string message;
		bool isCountItemsOk;

		message = $"Checking whether {countItems} of {totalExpected} expected is acceptable... ";

		if (!_options.ValidateCountItemsReturned)
		{
			// Items are always inside tolerance if validation is disabled
			isCountItemsOk = true;
			message += $"Yes - {nameof(_options.ValidateCountItemsReturned)} is disabled.";
		}
		else
		{
			isCountItemsOk = countItems >= totalExpected - _options.ValidateCountItemsReturnedTolerance
							  && countItems <= totalExpected + _options.ValidateCountItemsReturnedTolerance;
			message += $"{(isCountItemsOk ? "Yes - inside" : "No - outside")} the tolerance of {_options.ValidateCountItemsReturnedTolerance}.";
		}

		_logger.LogDebug(message);
		return isCountItemsOk;
	}

	#region GetAllByQueryAsync
	public Task<List<JObject>> GetAllByQueryAsync(string tableName)
		=> GetAllByQueryAsync(tableName, null, null, null, null, null, default);

	public Task<List<JObject>> GetAllByQueryAsync(string tableName, CancellationToken cancellationToken)
		=> GetAllByQueryAsync(tableName, null, null, null, null, null, cancellationToken);

	public Task<List<JObject>> GetAllByQueryAsync(string tableName, string query)
		=> GetAllByQueryAsync(tableName, query, null, null, null, null, default);
	public Task<List<JObject>> GetAllByQueryAsync(string tableName, string query, CancellationToken cancellationToken)
		=> GetAllByQueryAsync(tableName, query, null, null, null, null, cancellationToken);

	public Task<List<JObject>> GetAllByQueryAsync(string tableName, string? query, List<string>? fieldList)
		=> GetAllByQueryAsync(tableName, query, fieldList, null, null, null, default);

	public Task<List<JObject>> GetAllByQueryAsync(string tableName, string? query, List<string>? fieldList, CancellationToken cancellationToken)
		=> GetAllByQueryAsync(tableName, query, fieldList, null, null, null, cancellationToken);

	public Task<List<JObject>> GetAllByQueryAsync(string tableName, string? query, List<string>? fieldList, string? extraQueryString)
		=> GetAllByQueryAsync(tableName, query, fieldList, extraQueryString, null, null, default);

	public Task<List<JObject>> GetAllByQueryAsync(string tableName, string? query, List<string>? fieldList, string? extraQueryString, CancellationToken cancellationToken)
		=> GetAllByQueryAsync(tableName, query, fieldList, extraQueryString, null, null, cancellationToken);

	public Task<List<JObject>> GetAllByQueryAsync(string tableName, string? query, List<string>? fieldList, string? extraQueryString, string? customOrderByField)
		=> GetAllByQueryAsync(tableName, query, fieldList, extraQueryString, customOrderByField, null, default);

	public Task<List<JObject>> GetAllByQueryAsync(string tableName, string? query, List<string>? fieldList, string? extraQueryString, string? customOrderByField, int? pageSize)
		=> GetAllByQueryAsync(tableName, query, fieldList, extraQueryString, customOrderByField, pageSize, default);

	/// <summary>
	/// Get data by query
	/// </summary>
	/// <param name="tableName">Table name</param>
	/// <param name="query">Query</param>
	/// <param name="fieldList">Fields to retrieve</param>
	/// <param name="extraQueryString">Extra query string</param>
	/// <param name="customOrderByField">Optional field to order by when paging; default=sys_created_on</param>
	/// <param name="cancellationToken">CancellationToken</param>
	/// <returns></returns>
	public async Task<List<JObject>> GetAllByQueryAsync(
		string tableName,
		string? query,
		List<string>? fieldList,
		string? extraQueryString,
		string? customOrderByField,
		int? pageSize,
		CancellationToken cancellationToken)
	{
		_logger.LogDebug($"Calling {nameof(GetAllByQueryAsync)}" +
						 $" {nameof(tableName)}: {tableName}" +
						 $", {nameof(query)}: {query ?? "<not set>"}" +
						 $", {nameof(fieldList)}: {(fieldList?.Any() == true ? string.Join(", ", fieldList) : "<not set>")}" +
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
		var orderByField = string.IsNullOrWhiteSpace(customOrderByField)
			? _options.PagingFieldName
			: customOrderByField;

		string orderByCommand;
		if (orderByField?.StartsWith("-", StringComparison.Ordinal) ?? false)
		{
			orderByCommand = "ORDERBYDESC";
			orderByField = orderByField.Substring(1);
		}
		else
		{
			orderByCommand = "ORDERBY";
		}

		_logger.LogTrace($"Entered {nameof(GetAllByQueryInternalJObjectAsync)}" +
						 $" type: {typeof(JObject)}" +
						 $", {nameof(tableName)}: {tableName}" +
						 $", {nameof(query)}: {query ?? "<not set>"}" +
						 $", {nameof(fieldList)}: {(fieldList?.Any() == true ? string.Join(", ", fieldList) : "<not set>")}" +
						 $", {nameof(extraQueryString)}: {(string.IsNullOrWhiteSpace(extraQueryString) ? "<not set>" : extraQueryString)}" +
						 $", {nameof(orderByField)}: {(string.IsNullOrWhiteSpace(orderByField) ? "<not set>" : orderByField)}" +
						 $", PageSize: {pageSize}" +
						 ".");

		// NO - we are now using options because it's been discovered that not all tables/views will have
		// the below, and you can set to another field name if you want

		// Initialise actualFieldList from fieldList or an empty list if it was null
		var actualFieldList = new List<string>(fieldList ?? new List<string>());

		if (actualFieldList.Count > 0)
		{
			// Field list is provided so we need to make sure it includes the fields we need
			if (!actualFieldList.Contains(orderByField))
			{
				actualFieldList.Add(orderByField);
			}

			// sys_id is required for de-dupe during paging
			if (!actualFieldList.Contains("sys_id"))
			{
				actualFieldList.Add("sys_id");
			}
		}
		// To avoid issues with duplicates we HAVE to sort by something.

		// Does the query contain an ORDERBY?
		// Ordering: https://docs.servicenow.com/bundle/geneva-servicenow-platform/page/administer/exporting_data/reference/r_URLQueryParameters.html
		// Complain if ORDERBY has been provided
		if (query?.Contains("ORDERBY") == true)
		{
			throw new ServiceNowApiException("ORDERBY not supported in query due to paging mechanism");
		}

		// Set the ordering default
		if (query == null)
		{
			query = $"{orderByCommand}{orderByField}";
		}
		else
		{
			query += $"^{orderByCommand}{orderByField}";
		}

		// Strategy: We're ordering by the sys_created_on (by default, unless set to something else), so get the first page without limits and then subsequent pages based on >= the max time we got to make sure we don't miss any, need to remove duplicates
		DateTimeOffset maxDateTimeRetrieved;
		DateTimeOffset previousMaxDateTimeRetrieved;
		// This will be our final response
		var finalResult = new Page<JObject>();
		// While the last request returned at least the pageSize then we continue
		var apiReportedTotalCount = 0;
		var pagesRetrieved = 0;
		// Initially the queryWithPagingOffset will just be the original query
		var queryWithPagingOffset = query;
		while (true)
		{
			var response = await GetPageByQueryInternalAsync<JObject>(0, pageSize, tableName, queryWithPagingOffset, actualFieldList, extraQueryString, cancellationToken).ConfigureAwait(false);
			pagesRetrieved++;
			if (pagesRetrieved == 1)
			{
				// TotalCount will change each time as the criteria is changing so get it from the original query where we didn't change the criteria to include a date range
				apiReportedTotalCount = response.TotalCount;
			}

			// Add this response to the list
			finalResult.Items.AddRange(response.Items);
			_logger.LogTrace($"Last request received {response?.Items?.Count.ToString() ?? "UNKNOWN"} items");

			// If we got at least the number we asked for then there are probably more
			if (response?.Items?.Count == pageSize)
			{
				previousMaxDateTimeRetrieved = maxDateTimeRetrieved;

				if (response.Items.All(item => item[orderByField] is null))
				{
					// We cannot determine the paging based on this field name (which MAY NOT EXIST!)
					throw new ServiceNowApiException(
						$"The table / view '{tableName}' does not have the '{_options.PagingFieldName}' field " +
						"required to automatically page all the results. You could try a paged query instead.");
				}

				// At this point, we can be sure that we have the paging field in the data
				maxDateTimeRetrieved = response.Items.Max(jObject =>
					// Parse and enforce source as being UTC (Z)
					DateTimeOffset.Parse((jObject[orderByField]?.ToString() ?? string.Empty) + "Z"));

				if (previousMaxDateTimeRetrieved == maxDateTimeRetrieved)
				{
					throw new ServiceNowApiException("Paging window has not increased, try a larger page size.");
				}

				// Update the offset for the next query
				queryWithPagingOffset = $"{query}^{orderByField}>={maxDateTimeRetrieved.UtcDateTime:yyyy-MM-dd HH:mm:ss}";
				continue;
			}

			// All done
			break;
		}

		// https://community.servicenow.com/community?id=community_question&sys_id=bd7f8725dbdcdbc01dcaf3231f961949
		// See if we have any dupes based on sys_id
		if (finalResult.Items.Count > 0)
		{
			// Do we have a sys_id to work with
			// Need to dedupe as we might have got multiples due to paging mechanism
			var unique = new Dictionary<string, JObject>();
			foreach (var jObject in finalResult.Items)
			{
				unique[jObject["sys_id"]?.ToString() ?? string.Empty] = jObject;
			}

			finalResult.Items = unique.Values.ToList();
		}

		// If required, double check how many we got is how many we should have
		finalResult.TotalCount = apiReportedTotalCount;
		_logger.LogTrace($"Initial reported TotalCount from API: {apiReportedTotalCount}");
		if (!ItemsReturnedInsideTolerance(finalResult.Items.Count, finalResult.TotalCount))
		{
			throw new Exception($"Expected {finalResult.TotalCount:N0} {typeof(JObject)} but retrieved {finalResult.Items.Count:N0}");
		}

		// Are there any results
		if (finalResult.Items.Count > 0)
		{
			// YES - Did we specify any fields?
			if (fieldList?.Count > 0)
			{
				// YES - Make sure we only send back the fields we asked for
				var first = finalResult.Items[0];
				var actualPropertyNames = first.Properties().Select(p => p.Name).OrderBy(name => name).ToList();
				if (!fieldList.OrderBy(name => name).SequenceEqual(actualPropertyNames))
				{
					var propertiesToRemove = actualPropertyNames.Where(name => !fieldList.Contains(name)).ToList();
					foreach (var item in finalResult.Items)
					{
						foreach (var propertyName in propertiesToRemove)
						{
							_ = item.Remove(propertyName);
						}
					}
				}
			}
		}

		_logger.LogDebug($"Retrieved {finalResult.Items.Count:N0} items from ServiceNow.");
		return finalResult.Items;
	}
	#endregion

	#region GetPageByQueryAsync
	public Task<Page<JObject>> GetPageByQueryAsync(int skip, int take, string tableName)
		=> GetPageByQueryAsync(skip, take, tableName, null, null, null, default);

	public Task<Page<JObject>> GetPageByQueryAsync(int skip, int take, string tableName, CancellationToken cancellationToken)
		=> GetPageByQueryAsync(skip, take, tableName, null, null, null, cancellationToken);

	public Task<Page<JObject>> GetPageByQueryAsync(int skip, int take, string tableName, string query)
		=> GetPageByQueryAsync(skip, take, tableName, query, null, null, default);

	public Task<Page<JObject>> GetPageByQueryAsync(int skip, int take, string tableName, string query, CancellationToken cancellationToken)
		=> GetPageByQueryAsync(skip, take, tableName, query, null, null, default);

	public Task<Page<JObject>> GetPageByQueryAsync(int skip, int take, string tableName, string? query, List<string> fieldList)
		=> GetPageByQueryAsync(skip, take, tableName, query, fieldList, null, default);

	public Task<Page<JObject>> GetPageByQueryAsync(int skip, int take, string tableName, string? query, List<string> fieldList, CancellationToken cancellationToken)
		=> GetPageByQueryAsync(skip, take, tableName, query, fieldList, null, cancellationToken);

	public Task<Page<JObject>> GetPageByQueryAsync(int skip, int take, string tableName, string? query, List<string>? fieldList, string extraQueryString)
		=> GetPageByQueryAsync(skip, take, tableName, query, fieldList, extraQueryString, default);

	public Task<Page<JObject>> GetPageByQueryAsync(
		int skip,
		int take,
		string tableName,
		string? query,
		List<string>? fieldList,
		string? extraQueryString,
		CancellationToken cancellationToken)
		=> GetPageByQueryInternalAsync<JObject>(skip, take, tableName, query, fieldList, extraQueryString, cancellationToken);
	#endregion

	#region GetPageByQueryAsync<T>
	public Task<Page<T>> GetPageByQueryAsync<T>(int skip, int take) where T : Table
		=> GetPageByQueryAsync<T>(skip, take, null, default);

	public Task<Page<T>> GetPageByQueryAsync<T>(int skip, int take, CancellationToken cancellationToken) where T : Table
		=> GetPageByQueryAsync<T>(skip, take, null, cancellationToken);

	public Task<Page<T>> GetPageByQueryAsync<T>(int skip, int take, string query) where T : Table
		=> GetPageByQueryAsync<T>(skip, take, query, default);

	public Task<Page<T>> GetPageByQueryAsync<T>(
		int skip,
		int take,
		string? query,
		CancellationToken cancellationToken) where T : Table
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
						 $", {nameof(fieldList)}: {(fieldList?.Any() == true ? string.Join(", ", fieldList) : "<not set>")}" +
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
	#endregion

	private static string? BuildFieldListQueryParameter(List<string>? fieldList)
		=> fieldList?.Any() == true ? $"sysparm_fields={HttpUtility.UrlEncode(string.Join(",", fieldList))}" : null;

	#region GetByIdAsync<T>
	public Task<T?> GetByIdAsync<T>(string sysId) where T : Table
		=> GetByIdAsync<T>(sysId, default);

	public async Task<T?> GetByIdAsync<T>(string sysId, CancellationToken cancellationToken) where T : Table
		=> (await GetInternalAsync<RestResponse<T>>($"api/now/table/{Table.GetTableName<T>()}/{sysId}", cancellationToken).ConfigureAwait(false)).Item; 
	#endregion

	#region GetByIdAsync
	public Task<JObject?> GetByIdAsync(string tableName, string sysId)
		=> GetByIdAsync(tableName, sysId, default);

	public async Task<JObject?> GetByIdAsync(string tableName, string sysId, CancellationToken cancellationToken = default)
		=> (await GetInternalAsync<RestResponse<JObject>>($"api/now/table/{tableName}/{sysId}", cancellationToken).ConfigureAwait(false)).Item; 
	#endregion

	#region GetAttachmentsAsync<T>
	public Task<List<Attachment>> GetAttachmentsAsync<T>(T table) where T : Table
		=> GetAttachmentsAsync<T>(table, default);

	/// <summary>
	/// Get attachments for a given Table based entry
	/// </summary>
	/// <typeparam name="T">The type of object</typeparam>
	/// <param name="table">The object itself</param>
	/// <returns>A list of attachments</returns>
	public async Task<List<Attachment>> GetAttachmentsAsync<T>(T table, CancellationToken cancellationToken) where T : Table
		=> (await GetInternalAsync<RestResponse<List<Attachment>>>($"api/now/attachment?sysparm_query=table_name={Table.GetTableName<T>()}^table_sys_id={table.SysId}", cancellationToken).ConfigureAwait(false)).Item ?? new();
	#endregion

	#region GetAttachmentsAsync
	public Task<List<Attachment>> GetAttachmentsAsync(string tableName, string tableSysId)
		=> GetAttachmentsAsync(tableName, tableSysId, default);

	/// <summary>
	/// Get attachments for a given Table based entry
	/// </summary>
	/// <param name="tableName">The name of the table</param>
	/// <param name="tableSysId">The sys_id of the entry in the referenced table</param>
	/// <returns>A list of attachments</returns>
	public async Task<List<Attachment>> GetAttachmentsAsync(
		string tableName,
		string tableSysId,
		CancellationToken cancellationToken)
		=> (await GetInternalAsync<RestResponse<List<Attachment>>>($"api/now/attachment?sysparm_query=table_name={tableName}^table_sys_id={tableSysId}", cancellationToken).ConfigureAwait(false)).Item ?? new();
	#endregion

	#region DownloadAttachmentAsync
	public Task<string> DownloadAttachmentAsync(Attachment attachment, string outputPath)
		=> DownloadAttachmentAsync(attachment, outputPath, null, default);

	public Task<string> DownloadAttachmentAsync(Attachment attachment, string outputPath, CancellationToken cancellationToken)
		=> DownloadAttachmentAsync(attachment, outputPath, null, cancellationToken);

	public Task<string> DownloadAttachmentAsync(Attachment attachment, string outputPath, string filename)
		=> DownloadAttachmentAsync(attachment, outputPath, filename, default);

	/// <summary>
	/// Download a specified attachment to the local file system
	/// </summary>
	/// <param name="attachment">The attachment to download</param>
	/// <param name="outputPath">The path to store the attachment content in</param>
	/// <param name="filename">Optional filename for the file, defaults to filename from ServiceNow if unspecified</param>
	/// <returns>The path of the downloaded file</returns>
	public async Task<string> DownloadAttachmentAsync(
		Attachment attachment,
		string outputPath,
		string? filename,
		CancellationToken cancellationToken)
	{
		filename ??= attachment.FileName;
		var fileToWriteTo = Path.Combine(outputPath, filename);
		using var response = await _httpClient.GetAsync(attachment.DownloadLink, cancellationToken).ConfigureAwait(false);
		using var streamToReadFrom = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
		using Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create);
		await streamToReadFrom.CopyToAsync(streamToWriteTo).ConfigureAwait(false);
		response.Content = null;

		return fileToWriteTo;
	} 
	#endregion

	#region CreateAsync<T>
	public Task<T> CreateAsync<T>(T @object) where T : Table
		=> CreateAsync(@object, null, default);

	public Task<T> CreateAsync<T>(T @object, CancellationToken cancellationToken) where T : Table
		=> CreateAsync(@object, null, cancellationToken);

	public Task<T> CreateAsync<T>(T @object, string extraQueryString) where T : Table
		=> CreateAsync(@object, extraQueryString, default);

	public async Task<T> CreateAsync<T>(T @object, string? extraQueryString, CancellationToken cancellationToken) where T : Table
	{
		// https://docs.servicenow.com/bundle/kingston-application-development/page/integrate/inbound-rest/concept/c_TableAPI.html#ariaid-title6
		var serializedObject = JsonConvert.SerializeObject(@object, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
		HttpContent content = new StringContent(serializedObject, null, "application/json");
		var tableName = Table.GetTableName<T>();
		using var response = await _httpClient.PostAsync(
				$"api/now/table/{tableName}" + (string.IsNullOrWhiteSpace(extraQueryString) ? "" : "?" + extraQueryString),
				content,
				cancellationToken
			).ConfigureAwait(false)
			?? throw new Exception("Null response.");

		if (!response.IsSuccessStatusCode)
		{
			var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			throw new Exception($"Server error {response.StatusCode} ({(int)response.StatusCode}): {response.ReasonPhrase} - {responseContent}.");
		}

		var deserializeObject = await GetDeserializedObjectFromResponse<RestResponse<T>>(response, Guid.NewGuid()).ConfigureAwait(false);
		return deserializeObject.Item!;
	} 
	#endregion

	#region CreateAsync
	public Task<JObject> CreateAsync(string tableName, JObject jObject)
		=> CreateAsync(tableName, jObject, null, default);

	public Task<JObject> CreateAsync(string tableName, JObject jObject, CancellationToken cancellationToken)
		=> CreateAsync(tableName, jObject, null, cancellationToken);

	public Task<JObject> CreateAsync(string tableName, JObject jObject, string extraQueryString)
		=> CreateAsync(tableName, jObject, extraQueryString, default);

	public async Task<JObject> CreateAsync(string tableName, JObject jObject, string? extraQueryString, CancellationToken cancellationToken)
	{
		// https://docs.servicenow.com/bundle/kingston-application-development/page/integrate/inbound-rest/concept/c_TableAPI.html#ariaid-title6
		var serializedObject = JsonConvert.SerializeObject(jObject, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
		HttpContent content = new StringContent(serializedObject, null, "application/json");
		using var response = await _httpClient.PostAsync(
				$"api/now/table/{tableName}" + (string.IsNullOrWhiteSpace(extraQueryString) ? "" : "?" + extraQueryString),
				content,
				cancellationToken
			).ConfigureAwait(false)
			?? throw new Exception("Null response.");

		if (!response.IsSuccessStatusCode)
		{
			var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			throw new Exception($"Server error {response.StatusCode} ({(int)response.StatusCode}): {response.ReasonPhrase} - {responseContent}.");
		}

		var deserializeObject = await GetDeserializedObjectFromResponse<RestResponse<JObject>>(response, Guid.NewGuid()).ConfigureAwait(false);
		return deserializeObject.Item!;
	}
	#endregion

	#region UpdateAsync<T>
	public Task UpdateAsync<T>(T @object) where T : Table
		=> UpdateAsync(@object, default);

	public async Task<JObject> UpdateAsync(string tableName, JObject jObject, CancellationToken cancellationToken)
	{
		if (jObject == null)
		{
			throw new ArgumentNullException(nameof(jObject));
		}

		if (!jObject.TryGetValue("sys_id", out var sysId))
		{
			throw new ArgumentException($"sys_id must be present in the {nameof(jObject)} parameter.", nameof(jObject));
		}

		// https://docs.servicenow.com/bundle/kingston-application-development/page/integrate/inbound-rest/concept/c_TableAPI.html#ariaid-title6
		var serializedObject = JsonConvert.SerializeObject(jObject, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
		HttpContent content = new StringContent(serializedObject, null, "application/json");
		using var response = await _httpClient.PutAsync($"api/now/table/{tableName}/{sysId}", content, cancellationToken).ConfigureAwait(false)
			?? throw new Exception("Null response.");

		if (!response.IsSuccessStatusCode)
		{
			var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			throw new Exception($"Server error {response.StatusCode} ({(int)response.StatusCode}): {response.ReasonPhrase} - {responseContent}.");
		}

		var deserializeObject = await GetDeserializedObjectFromResponse<RestResponse<JObject>>(response, Guid.NewGuid()).ConfigureAwait(false);
		return deserializeObject.Item!;
	} 
	#endregion

	#region PatchAsync
	public Task<JObject> PatchAsync(string tableName, JObject jObject)
		=> PatchAsync(tableName, jObject, default);

	/// <summary>
	/// Patches an existing entry. jObject must contain sys_id
	/// </summary>
	/// <param name="tableName">The table to update an entry in</param>
	/// <param name="jObject">The object details, sys_id must be set</param>
	/// <param name="cancellationToken"></param>
	public async Task<JObject> PatchAsync(string tableName, JObject jObject, CancellationToken cancellationToken)
	{
		if (jObject == null)
		{
			throw new ArgumentNullException(nameof(jObject));
		}

		if (!jObject.TryGetValue("sys_id", out var sysId))
		{
			throw new ArgumentException($"sys_id must be present in the {nameof(jObject)} parameter.", nameof(jObject));
		}

		var serializedObject = JsonConvert.SerializeObject(jObject, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
		var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"api/now/table/{tableName}/{sysId}") { Content = new StringContent(serializedObject, null, "application/json") };

		using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false)
			?? throw new Exception("Null response.");

		if (!response.IsSuccessStatusCode)
		{
			var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			throw new Exception($"Server error {response.StatusCode} ({(int)response.StatusCode}): {response.ReasonPhrase} - {responseContent}.");
		}

		var deserializeObject = await GetDeserializedObjectFromResponse<RestResponse<JObject>>(response, Guid.NewGuid()).ConfigureAwait(false);
		return deserializeObject.Item!;
	} 
	#endregion

	#region DeleteAsync
	public Task DeleteAsync(string tableName, string sysId)
		=> DeleteAsync(tableName, sysId, default);

	public async Task DeleteAsync(string tableName, string sysId, CancellationToken cancellationToken = default)
	{
		// https://docs.servicenow.com/bundle/kingston-application-development/page/integrate/inbound-rest/concept/c_TableAPI.html#ariaid-title6
		using var response = await _httpClient.DeleteAsync($"api/now/table/{tableName}/{sysId}", cancellationToken).ConfigureAwait(false)
			?? throw new Exception("Null response.");

		if (!response.IsSuccessStatusCode)
		{
			var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			throw new Exception($"Server error {response.StatusCode} ({(int)response.StatusCode}): {response.ReasonPhrase} - {responseContent}.");
		}
	}
	#endregion

	#region UpdateAsync
	public Task<JObject> UpdateAsync(string tableName, JObject jObject)
		=> UpdateAsync(tableName, jObject, default);

	public async Task UpdateAsync<T>(T @object, CancellationToken cancellationToken) where T : Table
	{
		HttpContent content = new StringContent(JsonConvert.SerializeObject(@object), null, "application/json");
		var tableName = Table.GetTableName<T>();
		using var response = await _httpClient.PutAsync($"api/now/table/{tableName}/{@object.SysId}", content, cancellationToken).ConfigureAwait(false)
			?? throw new Exception("Null response.");

		if (!response.IsSuccessStatusCode)
		{
			var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			throw new Exception($"Server error {response.StatusCode} ({(int)response.StatusCode}): {response.ReasonPhrase} - {responseContent}.");
		}
	} 
	#endregion

	#region DeleteAsync<T>
	public Task DeleteAsync<T>(string sysId) where T : Table
		=> DeleteAsync<T>(sysId, default);

	public async Task DeleteAsync<T>(string sysId, CancellationToken cancellationToken) where T : Table
	{
		var tableName = Table.GetTableName<T>();

		using var response = await _httpClient.DeleteAsync($"api/now/table/{tableName}/{sysId}", cancellationToken).ConfigureAwait(false)
			?? throw new Exception("Null response.");

		if (!response.IsSuccessStatusCode)
		{
			var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			throw new Exception($"Server error {response.StatusCode} ({(int)response.StatusCode}): {response.ReasonPhrase} - {responseContent}.");
		}
	}
	#endregion

	#region GetMetaForClassAsync
	public Task<RestResponse<MetaDataResult>> GetMetaForClassAsync(string className)
		=> GetMetaForClassAsync(className, default);

	public Task<RestResponse<MetaDataResult>> GetMetaForClassAsync(string className, CancellationToken cancellationToken)
		=> GetInternalAsync<RestResponse<MetaDataResult>>($"api/now/cmdb/meta/{className}", cancellationToken); 
	#endregion

	private async Task<T> GetInternalAsync<T>(string subUrl, CancellationToken cancellationToken)
	{
		var requestId = Guid.NewGuid();
		_logger.LogTrace($"Request {requestId}: Entered {nameof(GetInternalAsync)} {nameof(subUrl)}: {subUrl}");
		var sw = Stopwatch.StartNew();
		using var response = await _httpClient.GetAsync(subUrl, cancellationToken).ConfigureAwait(false);
		_logger.LogTrace($"Request {requestId}: GetAsync took {sw.Elapsed}");

		if (response == null)
		{
			throw new Exception("Null response.");
		}

		if (!response.IsSuccessStatusCode)
		{
			var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			throw new Exception($"Server error {response.StatusCode} ({(int)response.StatusCode}): {response.ReasonPhrase} - {responseContent}.");
		}

		return await GetDeserializedObjectFromResponse<T>(response, requestId).ConfigureAwait(false);
	}

	private async Task<T> GetDeserializedObjectFromResponse<T>(HttpResponseMessage response, Guid requestId)
	{
		string? content = null;
		try
		{
			content = await response
				.Content
				.ReadAsStringAsync()
				.ConfigureAwait(false);

			_logger.LogTrace($"Request {requestId}: Content length: {content.Length / 1024:N0} KB");

			// Deserialize the object
			var deserializeObject = JsonConvert.DeserializeObject<T>(content);

			// If this is a list then we add on the TotalCount
			if (deserializeObject is RestListResponseBase restListResponse
				&& response.Headers.TryGetValues("X-Total-Count", out var values)
				)
			{
				// We really should have an X-Total-Count available when retrieving lists
				var totalCount = values.FirstOrDefault();
				// If we really don't have it then it will default to 0 as this is an int
				if (totalCount != null && int.TryParse(totalCount, out var totalCountInt))
				{
					restListResponse.TotalCount = totalCountInt;
					_logger.LogTrace($"Request {requestId}: X-Total-Count: {totalCountInt:N0}");
				}
			}

			return deserializeObject!;
		}
		catch (Exception e)
		{
			throw new ServiceNowApiException($"A problem occurred deserializing the content from the response. Content:\n{content ?? "<content not read>"}", e);
		}
	}

	#region GetLinkedEntityAsync
	public Task<JObject?> GetLinkedEntityAsync(string link, List<string> fieldList)
		=> GetLinkedEntityAsync(link, fieldList, default);

	public async Task<JObject?> GetLinkedEntityAsync(string link, List<string> fieldList, CancellationToken cancellationToken)
	{
		var linkWithFields = link.Substring(link.IndexOf("/api/", StringComparison.Ordinal) + 1);
		if (fieldList?.Any() == true)
		{
			linkWithFields += "?" + BuildFieldListQueryParameter(fieldList);
		}

		return (await GetInternalAsync<RestResponse<JObject>>(linkWithFields, cancellationToken).ConfigureAwait(false)).Item;
	} 
	#endregion

}
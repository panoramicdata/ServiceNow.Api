using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceNow.Api.Exceptions;
using ServiceNow.Api.MetaData;
using ServiceNow.Api.Tables;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace ServiceNow.Api;

public class ServiceNowClient : IDisposable
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
		ArgumentNullException.ThrowIfNull(account);

		ArgumentNullException.ThrowIfNull(username);

		ArgumentNullException.ThrowIfNull(password);

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
		var basicString = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
		_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicString);
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

	/// <summary>
	/// Test-only constructor accepting a message handler, so that paging and count-validation
	/// behaviour can be exercised deterministically without a live ServiceNow instance.
	/// </summary>
	/// <param name="httpMessageHandler">The handler to send requests through.</param>
	/// <param name="options">Client options.</param>
	internal ServiceNowClient(
		HttpMessageHandler httpMessageHandler,
		Options? options = null)
	{
		ArgumentNullException.ThrowIfNull(httpMessageHandler);

		_options = options ?? new();
		_logger = _options.Logger ?? new NullLogger<ServiceNowClient>();
		AccountName = "test";

		_httpClient = new HttpClient(httpMessageHandler)
		{
			BaseAddress = new Uri("https://test.service-now.com"),
			DefaultRequestHeaders =
			{
				Accept = {new MediaTypeWithQualityHeaderValue("application/json")},
			}
		};
	}

	public string AccountName { get; } = string.Empty;

	public void Dispose()
	{
		_httpClient?.Dispose();
		GC.SuppressFinalize(this);
	}

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
		_logger.LogTrace($"Entered {nameof(GetAllByQueryInternalAsync)} type: {typeof(T)}, {nameof(tableName)}: {tableName}, {nameof(query)}: {query ?? "<not set>"}, {nameof(fieldList)}: {(fieldList?.Any() == true ? string.Join(", ", fieldList) : "<not set>")}, {nameof(extraQueryString)}: {(string.IsNullOrWhiteSpace(extraQueryString) ? "<not set>" : extraQueryString)}.");
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
				throw new Exception($"Expected {finalResult.TotalCount:N0} items but only retrieved {finalResult.Items.Count:N0}, which is not within the {_options.ValidateCountItemsReturnedTolerance} tolerance.");
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
			// Only a SHORTFALL is enforced. Retrieving more than expected is not data loss:
			// the total is a point-in-time snapshot taken on the first page, and a walk over a
			// large table takes minutes, during which a live system legitimately gains records.
			// Requiring an exact match made every busy table fail intermittently.
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
						 $", {nameof(fieldList)}: {(fieldList?.Any() == true ? string.Join(", ", fieldList) : "<not set>")}" +
						 $", {nameof(extraQueryString)}: {(string.IsNullOrWhiteSpace(extraQueryString) ? "<not set>" : extraQueryString)}" +
						 $", {nameof(customOrderByField)}: {(string.IsNullOrWhiteSpace(customOrderByField) ? "<not set>" : customOrderByField)}" +
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
		var orderByField = string.IsNullOrWhiteSpace(customOrderByField)
			? _options.PagingFieldName
			: customOrderByField;

		string orderByCommand;
		if (orderByField?.StartsWith('-') ?? false)
		{
			orderByCommand = "ORDERBYDESC";
			orderByField = orderByField[1..];
		}
		else
		{
			orderByCommand = "ORDERBY";
		}

		// Ensure orderByField is not null for subsequent operations
		orderByField ??= _options.PagingFieldName;

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
		var actualFieldList = new List<string>(fieldList ?? []);

		if (actualFieldList.Count > 0)
		{
			// Field list is provided so we need to make sure it includes the fields we need
			if (!string.IsNullOrEmpty(orderByField) && !actualFieldList.Contains(orderByField))
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
		var maxDateTimeRetrieved = DateTimeOffset.MinValue;
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

			var items = response?.Items ?? [];

			// Add this response to the list
			finalResult.Items.AddRange(items);
			_logger.LogTrace($"Last request received {items.Count} items");

			// An EMPTY page is the only reliable end-of-data signal.
			//
			// A short page is NOT. ServiceNow applies read ACLs AFTER sysparm_limit, so a
			// request for 1,000 rows can legitimately return fewer while a great deal of data
			// still follows. This previously terminated paging early and silently dropped the
			// remainder: on one customer query a 997-row page at position 49 of 88 ended the
			// walk at 48,724 of 86,540 rows, a 44% loss with no error raised.
			if (items.Count == 0)
			{
				break;
			}

			previousMaxDateTimeRetrieved = maxDateTimeRetrieved;

			if (items.All(item => item[orderByField!] is null))
			{
				// Without the paging field the window cannot advance, but that only matters if records
				// remain: a result that fits in one page needs no paging, and many views carry no
				// date/time field. Completeness uses the same tolerance rule as the finished walk, so a
				// genuine shortfall still raises rather than truncating silently.
				var pageWasFull = items.Count == pageSize;
				if (!pageWasFull && ItemsReturnedInsideTolerance(finalResult.Items.Count, apiReportedTotalCount))
				{
					break;
				}

				throw new ServiceNowApiException(
					$"The table / view '{tableName}' does not have the '{orderByField}' field, which is " +
					$"required to page through the full result set ({finalResult.Items.Count:N0} of " +
					$"{apiReportedTotalCount:N0} records retrieved). Set the {nameof(Options.PagingFieldName)} " +
					"option (or the customOrderByField parameter) to a date/time field that this table / view " +
					"does have, or request a single page with skip / take.");
			}

			// At this point, we can be sure that we have the paging field in the data
			maxDateTimeRetrieved = items.Max(jObject => ParsePagingFieldValue(jObject, orderByField!, tableName));

			if (previousMaxDateTimeRetrieved == maxDateTimeRetrieved)
			{
				// The window cannot advance. A FULL page means genuinely more records share
				// this one timestamp than the page size can carry, which needs a larger page.
				if (items.Count == pageSize)
				{
					throw new ServiceNowApiException("Paging window has not increased, try a larger page size.");
				}

				// Otherwise this is just the boundary record being re-read by the '>=' window,
				// so there is nothing further to collect.
				break;
			}

			// Update the offset for the next query
			queryWithPagingOffset = $"{query}^{orderByField}>={maxDateTimeRetrieved.UtcDateTime:yyyy-MM-dd HH:mm:ss}";
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

			finalResult.Items = [.. unique.Values];
		}

		// If required, double check how many we got is how many we should have
		finalResult.TotalCount = apiReportedTotalCount;
		_logger.LogTrace($"Initial reported TotalCount from API: {apiReportedTotalCount}");
		if (!ItemsReturnedInsideTolerance(finalResult.Items.Count, finalResult.TotalCount))
		{
			throw new Exception($"Expected {finalResult.TotalCount:N0} items but retrieved {finalResult.Items.Count:N0}, which is not within the {_options.ValidateCountItemsReturnedTolerance} tolerance.");
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

	/// <summary>
	/// Reads the ordering field out of a returned row and converts it to a UTC DateTimeOffset, for use as the
	/// paging window boundary.
	/// </summary>
	/// <remarks>
	/// Two things make this less straightforward than it looks, both caused by sysparm_display_value.
	///
	/// With sysparm_display_value=all every field is returned as an object of the form
	/// { "display_value": ..., "value": ... } rather than a scalar, so calling ToString() on it yields JSON.
	/// The raw "value" is preferred here, because it carries the underlying UTC timestamp.
	///
	/// The value is also parsed with the invariant culture rather than the host's, since a display-formatted
	/// date such as 04/08/2026 would otherwise be interpreted differently depending on where the code runs,
	/// producing a wrong window rather than an error. AssumeUniversal replaces the previous approach of
	/// concatenating "Z" onto the string, which corrupted any value that already carried an offset.
	/// </remarks>
	private static DateTimeOffset ParsePagingFieldValue(JObject jObject, string orderByField, string tableName)
	{
		var token = jObject[orderByField];

		// sysparm_display_value=all returns { display_value, value }: prefer the raw value.
		if (token is JObject valueObject)
		{
			token = valueObject["value"] ?? valueObject["display_value"];
		}

		// Newtonsoft recognises ISO-8601 text during deserialisation and converts it to a date value before
		// we ever see it. Calling ToString() on that would render it in the HOST's culture and timezone,
		// which then reads back wrongly: an en-GB host turns 2026-01-02T05:00:00+05:00 into "02/01/2026
		// 07:00:00", which the invariant culture reads as 1 February. Take the value as a date directly.
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

	private static string? BuildFieldListQueryParameter(List<string>? fieldList)
		=> fieldList?.Any() == true ? $"sysparm_fields={HttpUtility.UrlEncode(string.Join(",", fieldList))}" : null;

	public async Task<T?> GetByIdAsync<T>(string sysId, CancellationToken cancellationToken = default) where T : Table
		=> (await GetInternalAsync<RestResponse<T>>($"api/now/table/{Table.GetTableName<T>()}/{sysId}", cancellationToken).ConfigureAwait(false)).Item;

	public async Task<JObject?> GetByIdAsync(string tableName, string sysId, CancellationToken cancellationToken = default)
		=> (await GetInternalAsync<RestResponse<JObject>>($"api/now/table/{tableName}/{sysId}", cancellationToken).ConfigureAwait(false)).Item;

	/// <summary>
	/// Get attachments for a given Table based entry
	/// </summary>
	/// <typeparam name="T">The type of object</typeparam>
	/// <param name="table">The object itself</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>A list of attachments</returns>
	public async Task<List<Attachment>> GetAttachmentsAsync<T>(T table, CancellationToken cancellationToken = default) where T : Table
		=> (await GetInternalAsync<RestResponse<List<Attachment>>>($"api/now/attachment?sysparm_query=table_name={Table.GetTableName<T>()}^table_sys_id={table.SysId}", cancellationToken).ConfigureAwait(false)).Item ?? [];

	/// <summary>
	/// Get attachments for a given Table based entry
	/// </summary>
	/// <param name="tableName">The name of the table</param>
	/// <param name="tableSysId">The sys_id of the entry in the referenced table</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>A list of attachments</returns>
	public async Task<List<Attachment>> GetAttachmentsAsync(
		string tableName,
		string tableSysId,
		CancellationToken cancellationToken = default)
		=> (await GetInternalAsync<RestResponse<List<Attachment>>>($"api/now/attachment?sysparm_query=table_name={tableName}^table_sys_id={tableSysId}", cancellationToken).ConfigureAwait(false)).Item ?? [];

	/// <summary>
	/// Download a specified attachment to the local file system
	/// </summary>
	/// <param name="attachment">The attachment to download</param>
	/// <param name="outputPath">The path to store the attachment content in</param>
	/// <param name="filename">Optional filename for the file, defaults to filename from ServiceNow if unspecified</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>The path of the downloaded file</returns>
	public async Task<string> DownloadAttachmentAsync(
		Attachment attachment,
		string outputPath,
		string? filename = null,
		CancellationToken cancellationToken = default)
	{
		var actualFilename = filename ?? attachment.FileName ?? throw new ArgumentException("Filename must be provided or available from attachment", nameof(filename));
		var fileToWriteTo = Path.Combine(outputPath, actualFilename);
		using var response = await _httpClient.GetAsync(attachment.DownloadLink, cancellationToken).ConfigureAwait(false);
		using var streamToReadFrom = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
		using Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create);
		await streamToReadFrom.CopyToAsync(streamToWriteTo, cancellationToken).ConfigureAwait(false);
		response.Content = null;

		return fileToWriteTo;
	}

	public Task<T> CreateAsync<T>(T @object, CancellationToken cancellationToken = default) where T : Table
		=> CreateAsync(@object, null, cancellationToken);

	public async Task<T> CreateAsync<T>(T @object, string? extraQueryString = null, CancellationToken cancellationToken = default) where T : Table
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
			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
			throw new Exception($"Server error {response.StatusCode} ({(int)response.StatusCode}): {response.ReasonPhrase} - {responseContent}.");
		}

		var deserializeObject = await GetDeserializedObjectFromResponse<RestResponse<T>>(response, Guid.NewGuid()).ConfigureAwait(false);
		return deserializeObject.Item!;
	}

	public Task<JObject> CreateAsync(string tableName, JObject jObject, CancellationToken cancellationToken = default)
		=> CreateAsync(tableName, jObject, null, cancellationToken);

	public async Task<JObject> CreateAsync(string tableName, JObject jObject, string? extraQueryString = null, CancellationToken cancellationToken = default)
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
			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
			throw new Exception($"Server error {response.StatusCode} ({(int)response.StatusCode}): {response.ReasonPhrase} - {responseContent}.");
		}

		var deserializeObject = await GetDeserializedObjectFromResponse<RestResponse<JObject>>(response, Guid.NewGuid()).ConfigureAwait(false);
		return deserializeObject.Item!;
	}

	public async Task<JObject> UpdateAsync(string tableName, JObject jObject, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(jObject);

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
			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
			throw new Exception($"Server error {response.StatusCode} ({(int)response.StatusCode}): {response.ReasonPhrase} - {responseContent}.");
		}

		var deserializeObject = await GetDeserializedObjectFromResponse<RestResponse<JObject>>(response, Guid.NewGuid()).ConfigureAwait(false);
		return deserializeObject.Item!;
	}

	/// <summary>
	/// Patches an existing entry. jObject must contain sys_id
	/// </summary>
	/// <param name="tableName">The table to update an entry in</param>
	/// <param name="jObject">The object details, sys_id must be set</param>
	/// <param name="cancellationToken"></param>
	public async Task<JObject> PatchAsync(string tableName, JObject jObject, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(jObject);

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
			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
			throw new Exception($"Server error {response.StatusCode} ({(int)response.StatusCode}): {response.ReasonPhrase} - {responseContent}.");
		}

		var deserializeObject = await GetDeserializedObjectFromResponse<RestResponse<JObject>>(response, Guid.NewGuid()).ConfigureAwait(false);
		return deserializeObject.Item!;
	}

	public async Task DeleteAsync(string tableName, string sysId, CancellationToken cancellationToken = default)
	{
		// https://docs.servicenow.com/bundle/kingston-application-development/page/integrate/inbound-rest/concept/c_TableAPI.html#ariaid-title6
		using var response = await _httpClient.DeleteAsync($"api/now/table/{tableName}/{sysId}", cancellationToken).ConfigureAwait(false)
			?? throw new Exception("Null response.");

		if (!response.IsSuccessStatusCode)
		{
			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
			throw new Exception($"Server error {response.StatusCode} ({(int)response.StatusCode}): {response.ReasonPhrase} - {responseContent}.");
		}
	}

	public async Task UpdateAsync<T>(T @object, CancellationToken cancellationToken = default) where T : Table
	{
		HttpContent content = new StringContent(JsonConvert.SerializeObject(@object), null, "application/json");
		var tableName = Table.GetTableName<T>();
		using var response = await _httpClient.PutAsync($"api/now/table/{tableName}/{@object.SysId}", content, cancellationToken).ConfigureAwait(false)
			?? throw new Exception("Null response.");

		if (!response.IsSuccessStatusCode)
		{
			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
			throw new Exception($"Server error {response.StatusCode} ({(int)response.StatusCode}): {response.ReasonPhrase} - {responseContent}.");
		}
	}

	public async Task DeleteAsync<T>(string sysId, CancellationToken cancellationToken = default) where T : Table
	{
		var tableName = Table.GetTableName<T>();

		using var response = await _httpClient.DeleteAsync($"api/now/table/{tableName}/{sysId}", cancellationToken).ConfigureAwait(false)
			?? throw new Exception("Null response.");

		if (!response.IsSuccessStatusCode)
		{
			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
			throw new Exception($"Server error {response.StatusCode} ({(int)response.StatusCode}): {response.ReasonPhrase} - {responseContent}.");
		}
	}

	public Task<RestResponse<MetaDataResult>> GetMetaForClassAsync(string className, CancellationToken cancellationToken = default)
		=> GetInternalAsync<RestResponse<MetaDataResult>>($"api/now/cmdb/meta/{className}", cancellationToken);

	private async Task<T> GetInternalAsync<T>(string subUrl, CancellationToken cancellationToken)
	{
		var requestId = Guid.NewGuid();
		_logger.LogTrace("Request {RequestId}: Entered {MethodName} {ParamName}: {SubUrl}", requestId, nameof(GetInternalAsync), nameof(subUrl), subUrl);
		var sw = Stopwatch.StartNew();
		using var response = await _httpClient.GetAsync(subUrl, cancellationToken).ConfigureAwait(false);
		_logger.LogTrace("Request {RequestId}: GetAsync took {Elapsed}", requestId, sw.Elapsed);

		if (response == null)
		{
			throw new Exception("Null response.");
		}

		if (!response.IsSuccessStatusCode)
		{
			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
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

	public async Task<JObject?> GetLinkedEntityAsync(string link, List<string> fieldList, CancellationToken cancellationToken = default)
	{
		var linkWithFields = link[(link.IndexOf("/api/", StringComparison.Ordinal) + 1)..];
		if (fieldList?.Any() == true)
		{
			linkWithFields += "?" + BuildFieldListQueryParameter(fieldList);
		}

		return (await GetInternalAsync<RestResponse<JObject>>(linkWithFields, cancellationToken).ConfigureAwait(false)).Item;
	}
}
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ServiceNow.Api.Exceptions;
using ServiceNow.Api.Tables;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Web;

namespace ServiceNow.Api;

public partial class ServiceNowClient
{
	public Task<Page<JObject>> GetPageByQueryAsync(
		int skip,
		int take,
		string tableName,
		[Optional] string? query,
		[Optional] List<string>? fieldList,
		[Optional] string? extraQueryString,
		[Optional] CancellationToken cancellationToken)
		=> GetPageByQueryInternalAsync<JObject>(skip, take, tableName, query, fieldList, extraQueryString, cancellationToken);

	public Task<Page<T>> GetPageByQueryAsync<T>(
		int skip,
		int take,
		[Optional] string? query,
		[Optional] CancellationToken cancellationToken) where T : Table
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
						 $" type: {typeof(T)}, {DescribeQuery(tableName, query, fieldList, null)}" +
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

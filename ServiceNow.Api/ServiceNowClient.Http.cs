using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServiceNow.Api.Exceptions;
using System.Diagnostics;
using System.Web;

namespace ServiceNow.Api;

public partial class ServiceNowClient
{
	private static string? BuildFieldListQueryParameter(List<string>? fieldList)
		=> fieldList?.Any() == true ? $"sysparm_fields={HttpUtility.UrlEncode(string.Join(",", fieldList))}" : null;

	private static string Describe(string? value)
		=> string.IsNullOrWhiteSpace(value) ? "<not set>" : value;

	private static string DescribeQuery(string tableName, string? query, List<string>? fieldList, string? extraQueryString)
		=> $"{nameof(tableName)}: {tableName}, {nameof(query)}: {query ?? "<not set>"}, {nameof(fieldList)}: {Describe(fieldList)}, {nameof(extraQueryString)}: {Describe(extraQueryString)}";

	private static string Describe(List<string>? fieldList)
		=> fieldList?.Any() == true ? string.Join(", ", fieldList) : "<not set>";

	private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
	{
		if (response.IsSuccessStatusCode)
		{
			return;
		}

		var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
		throw new Exception($"Server error {response.StatusCode} ({(int)response.StatusCode}): {response.ReasonPhrase} - {responseContent}.");
	}

	private async Task<HttpResponseMessage> SendCheckedAsync(HttpMethod method, string url, string? json, CancellationToken cancellationToken)
	{
		using var request = new HttpRequestMessage(method, url);
		if (json is not null)
		{
			request.Content = new StringContent(json, null, "application/json");
		}

		var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false)
			?? throw new Exception("Null response.");
		try
		{
			await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
		}
		catch
		{
			response.Dispose();
			throw;
		}

		return response;
	}

	private async Task SendAsync(HttpMethod method, string url, string? json, CancellationToken cancellationToken)
	{
		using var response = await SendCheckedAsync(method, url, json, cancellationToken).ConfigureAwait(false);
	}

	private async Task<TItem> SendForItemAsync<TItem>(HttpMethod method, string url, string json, CancellationToken cancellationToken)
	{
		using var response = await SendCheckedAsync(method, url, json, cancellationToken).ConfigureAwait(false);
		var deserialized = await GetDeserializedObjectFromResponse<RestResponse<TItem>>(response, Guid.NewGuid()).ConfigureAwait(false);
		return deserialized.Item!;
	}

	private async Task<T> GetInternalAsync<T>(string subUrl, CancellationToken cancellationToken)
	{
		var requestId = Guid.NewGuid();
		_logger.LogTrace("Request {RequestId}: Entered {MethodName} {ParamName}: {SubUrl}", requestId, nameof(GetInternalAsync), nameof(subUrl), subUrl);
		var sw = Stopwatch.StartNew();
		using var response = await _httpClient.GetAsync(subUrl, cancellationToken).ConfigureAwait(false);
		_logger.LogTrace("Request {RequestId}: GetAsync took {Elapsed}", requestId, sw.Elapsed);

		await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

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

			// An empty or "null" body is a valid response from some endpoints, so give callers an empty envelope
			var deserializeObject = JsonConvert.DeserializeObject<T>(content);
			if (deserializeObject is null && typeof(T).GetConstructor(Type.EmptyTypes) is not null)
			{
				deserializeObject = Activator.CreateInstance<T>();
			}

			if (deserializeObject is RestListResponseBase restListResponse)
			{
				ApplyTotalCount(restListResponse, response, requestId);
			}

			return deserializeObject!;
		}
		catch (Exception e)
		{
			throw new ServiceNowApiException($"A problem occurred deserializing the content from the response. Content:\n{content ?? "<content not read>"}", e);
		}
	}

	private void ApplyTotalCount(RestListResponseBase restListResponse, HttpResponseMessage response, Guid requestId)
	{
		// TotalCount stays 0 when the header is absent or unparseable
		if (response.Headers.TryGetValues("X-Total-Count", out var values)
			&& int.TryParse(values.FirstOrDefault(), out var totalCountInt))
		{
			restListResponse.TotalCount = totalCountInt;
			_logger.LogTrace($"Request {requestId}: X-Total-Count: {totalCountInt:N0}");
		}
	}
}

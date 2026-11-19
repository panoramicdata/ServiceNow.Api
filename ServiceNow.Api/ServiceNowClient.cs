using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceNow.Api.MetaData;
using ServiceNow.Api.Tables;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;

namespace ServiceNow.Api;

public partial class ServiceNowClient : IDisposable
{
	private static readonly JsonSerializerSettings _ignoreNullsSettings = new() { NullValueHandling = NullValueHandling.Ignore };

	private readonly ILogger _logger;
	private readonly HttpClient _httpClient;
	private readonly Options _options;

	public ServiceNowClient(
		string account,
		string username,
		string password,
		[Optional] Options? options)
		: this(account, CreateBasicHeader(account, username, password), options)
	{
	}

	/// <summary>
	/// Creates a client that sends the supplied Authorization header with every request.
	/// </summary>
	/// <remarks>
	/// The header is used as given and is never refreshed, so a bearer token is only valid until it expires.
	/// This does not implement the OAuth flow itself.
	/// </remarks>
	/// <param name="account">The ServiceNow instance name.</param>
	/// <param name="authenticationHeader">The Authorization header to send, for example a bearer token.</param>
	/// <param name="options">Client options.</param>
	public ServiceNowClient(
		string account,
		AuthenticationHeaderValue authenticationHeader,
		[Optional] Options? options)
	{
		_options = options ?? new();

		// Accept the ILogger passed in on options or create a NullLogger
		_logger = _options.Logger ?? new NullLogger<ServiceNowClient>();

		AccountName = account;
		ArgumentNullException.ThrowIfNull(account);

		ArgumentNullException.ThrowIfNull(authenticationHeader);

		var baseAddress = _options.Environment == ServiceNowEnvironment.GCC
			? $"https://{account}.servicenowservices.com"
			: $"https://{account}.service-now.com";
		_httpClient = CreateHttpClient(_options.HttpMessageHandler ?? new HttpClientHandler(), baseAddress);
		_httpClient.DefaultRequestHeaders.Authorization = authenticationHeader;
		_logger.LogDebug("Created ServiceNowClient instance.");
	}

	public ServiceNowClient(
		string account,
		string username,
		string password,
		[Optional] ILogger? iLogger)
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
		[Optional] Options? options)
	{
		ArgumentNullException.ThrowIfNull(httpMessageHandler);

		_options = options ?? new();
		_logger = _options.Logger ?? new NullLogger<ServiceNowClient>();
		AccountName = "test";

		_httpClient = CreateHttpClient(httpMessageHandler, "https://test.service-now.com");
	}

	public string AccountName { get; } = string.Empty;

	private static AuthenticationHeaderValue CreateBasicHeader(string account, string username, string password)
	{
		ArgumentNullException.ThrowIfNull(account);
		ArgumentNullException.ThrowIfNull(username);
		ArgumentNullException.ThrowIfNull(password);

		return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}")));
	}

	public void Dispose()
	{
		_httpClient?.Dispose();
		GC.SuppressFinalize(this);
	}

	public async Task<T?> GetByIdAsync<T>(string sysId, [Optional] CancellationToken cancellationToken) where T : Table
		=> (await GetInternalAsync<RestResponse<T>>($"api/now/table/{Table.GetTableName<T>()}/{sysId}", cancellationToken).ConfigureAwait(false)).Item;

	public async Task<JObject?> GetByIdAsync(string tableName, string sysId, [Optional] CancellationToken cancellationToken)
		=> (await GetInternalAsync<RestResponse<JObject>>($"api/now/table/{tableName}/{sysId}", cancellationToken).ConfigureAwait(false)).Item;

	/// <summary>
	/// Get attachments for a given Table based entry
	/// </summary>
	/// <typeparam name="T">The type of object</typeparam>
	/// <param name="table">The object itself</param>
	/// <returns>A list of attachments</returns>
	public Task<List<Attachment>> GetAttachmentsAsync<T>(T table) where T : Table
		=> GetAttachmentsAsync(table, CancellationToken.None);

	/// <summary>
	/// Get attachments for a given Table based entry
	/// </summary>
	/// <typeparam name="T">The type of object</typeparam>
	/// <param name="table">The object itself</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>A list of attachments</returns>
	public Task<List<Attachment>> GetAttachmentsAsync<T>(T table, CancellationToken cancellationToken) where T : Table
		=> GetAttachmentsAsync(Table.GetTableName<T>(), table.SysId, cancellationToken);

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
		[Optional] CancellationToken cancellationToken)
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
		[Optional] string? filename,
		[Optional] CancellationToken cancellationToken)
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

	/// <summary>
	/// Upload an attachment to a record using the Attachment API (api/now/attachment/file).
	/// The stream is sent as the raw request body, which is what that endpoint expects.
	/// </summary>
	/// <param name="tableName">The name of the table the record is in</param>
	/// <param name="tableSysId">The sys_id of the record to attach the file to</param>
	/// <param name="fileName">The file name to store the attachment under</param>
	/// <param name="content">The file content</param>
	/// <param name="contentType">The content type, defaults to application/octet-stream</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>The created attachment metadata</returns>
	public async Task<Attachment?> UploadAttachmentAsync(
		string tableName,
		string tableSysId,
		string fileName,
		Stream content,
		[Optional] string? contentType,
		[Optional] CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
		ArgumentException.ThrowIfNullOrWhiteSpace(tableSysId);
		ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
		ArgumentNullException.ThrowIfNull(content);

		var url = $"api/now/attachment/file?table_name={HttpUtility.UrlEncode(tableName)}&table_sys_id={HttpUtility.UrlEncode(tableSysId)}&file_name={HttpUtility.UrlEncode(fileName)}";
		using var request = new HttpRequestMessage(HttpMethod.Post, url)
		{
			Content = new StreamContent(content)
		};
		request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);

		using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
		await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
		return (await GetDeserializedObjectFromResponse<RestResponse<Attachment>>(response, Guid.NewGuid()).ConfigureAwait(false)).Item;
	}

	public Task<T> CreateAsync<T>(T @object, [Optional] CancellationToken cancellationToken) where T : Table
		=> CreateAsync(@object, null, cancellationToken);

	public Task<T> CreateAsync<T>(T @object, string? extraQueryString) where T : Table
		=> CreateAsync(@object, extraQueryString, CancellationToken.None);

	public Task<T> CreateAsync<T>(T @object, string? extraQueryString, CancellationToken cancellationToken) where T : Table
		=> SendForItemAsync<T>(HttpMethod.Post, CreateUrl(Table.GetTableName<T>(), extraQueryString), SerializeIgnoringNulls(@object), cancellationToken);

	public Task<JObject> CreateAsync(string tableName, JObject jObject, [Optional] CancellationToken cancellationToken)
		=> CreateAsync(tableName, jObject, null, cancellationToken);

	public Task<JObject> CreateAsync(string tableName, JObject jObject, string? extraQueryString)
		=> CreateAsync(tableName, jObject, extraQueryString, CancellationToken.None);

	public Task<JObject> CreateAsync(string tableName, JObject jObject, string? extraQueryString, CancellationToken cancellationToken)
		=> SendForItemAsync<JObject>(HttpMethod.Post, CreateUrl(tableName, extraQueryString), SerializeIgnoringNulls(jObject), cancellationToken);

	public Task<JObject> UpdateAsync(string tableName, JObject jObject)
		=> UpdateAsync(tableName, jObject, CancellationToken.None);

	public Task<JObject> UpdateAsync(string tableName, JObject jObject, CancellationToken cancellationToken)
		=> SendEntryAsync(HttpMethod.Put, tableName, jObject, cancellationToken);

	/// <summary>
	/// Patches an existing entry. jObject must contain sys_id
	/// </summary>
	/// <param name="tableName">The table to update an entry in</param>
	/// <param name="jObject">The object details, sys_id must be set</param>
	public Task<JObject> PatchAsync(string tableName, JObject jObject)
		=> PatchAsync(tableName, jObject, CancellationToken.None);

	/// <summary>
	/// Patches an existing entry. jObject must contain sys_id
	/// </summary>
	/// <param name="tableName">The table to update an entry in</param>
	/// <param name="jObject">The object details, sys_id must be set</param>
	/// <param name="cancellationToken"></param>
	public Task<JObject> PatchAsync(string tableName, JObject jObject, CancellationToken cancellationToken)
		=> SendEntryAsync(HttpMethod.Patch, tableName, jObject, cancellationToken);

	public Task DeleteAsync(string tableName, string sysId)
		=> DeleteAsync(tableName, sysId, CancellationToken.None);

	public Task DeleteAsync(string tableName, string sysId, CancellationToken cancellationToken)
		=> SendAsync(HttpMethod.Delete, $"api/now/table/{tableName}/{sysId}", null, cancellationToken);

	public Task UpdateAsync<T>(T @object) where T : Table
		=> UpdateAsync(@object, CancellationToken.None);

	public Task UpdateAsync<T>(T @object, CancellationToken cancellationToken) where T : Table
		=> SendAsync(HttpMethod.Put, $"api/now/table/{Table.GetTableName<T>()}/{@object.SysId}", JsonConvert.SerializeObject(@object), cancellationToken);

	public Task DeleteAsync<T>(string sysId) where T : Table
		=> DeleteAsync<T>(sysId, CancellationToken.None);

	public Task DeleteAsync<T>(string sysId, CancellationToken cancellationToken) where T : Table
		=> DeleteAsync(Table.GetTableName<T>(), sysId, cancellationToken);

	public Task<RestResponse<MetaDataResult>> GetMetaForClassAsync(string className)
		=> GetMetaForClassAsync(className, CancellationToken.None);

	public Task<RestResponse<MetaDataResult>> GetMetaForClassAsync(string className, CancellationToken cancellationToken)
		=> GetInternalAsync<RestResponse<MetaDataResult>>($"api/now/cmdb/meta/{className}", cancellationToken);

	public Task<JObject?> GetLinkedEntityAsync(string link, List<string> fieldList)
		=> GetLinkedEntityAsync(link, fieldList, CancellationToken.None);

	public async Task<JObject?> GetLinkedEntityAsync(string link, List<string> fieldList, CancellationToken cancellationToken)
	{
		var linkWithFields = link[(link.IndexOf("/api/", StringComparison.Ordinal) + 1)..];
		if (fieldList?.Any() == true)
		{
			linkWithFields += "?" + BuildFieldListQueryParameter(fieldList);
		}

		return (await GetInternalAsync<RestResponse<JObject>>(linkWithFields, cancellationToken).ConfigureAwait(false)).Item;
	}

	private HttpClient CreateHttpClient(HttpMessageHandler innerHandler, string baseAddress)
	{
		var httpClient = new HttpClient(new ReadOnlyHandler(_options, innerHandler))
		{
			BaseAddress = new Uri(baseAddress),
			DefaultRequestHeaders =
			{
				Accept = {new MediaTypeWithQualityHeaderValue("application/json")},
			}
		};
		if (_options.Timeout is { } timeout)
		{
			httpClient.Timeout = timeout;
		}

		return httpClient;
	}

	private static string CreateUrl(string tableName, string? extraQueryString)
		=> $"api/now/table/{tableName}" + (string.IsNullOrWhiteSpace(extraQueryString) ? "" : "?" + extraQueryString);

	private static string SerializeIgnoringNulls(object value)
		=> JsonConvert.SerializeObject(value, _ignoreNullsSettings);

	private async Task<JObject> SendEntryAsync(HttpMethod method, string tableName, JObject jObject, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(jObject);

		return jObject.TryGetValue("sys_id", out var sysId)
			? await SendForItemAsync<JObject>(method, $"api/now/table/{tableName}/{sysId}", SerializeIgnoringNulls(jObject), cancellationToken).ConfigureAwait(false)
			: throw new ArgumentException($"sys_id must be present in the {nameof(jObject)} parameter.", nameof(jObject));
	}
}

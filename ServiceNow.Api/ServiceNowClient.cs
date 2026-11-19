using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceNow.Api.MetaData;
using ServiceNow.Api.Tables;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;

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
}

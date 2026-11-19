using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceNow.Api.Tables;
using System.Runtime.InteropServices;

namespace ServiceNow.Api;

public partial class ServiceNowClient
{
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

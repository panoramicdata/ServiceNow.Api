using Newtonsoft.Json.Linq;
using Xunit;

namespace ServiceNow.Api.Test;

/// <summary>
/// Runs GetAllByQueryAsync against a stub handler, so stubbed tests share one client lifecycle.
/// </summary>
internal static class StubRun
{
	public static async Task<List<JObject>> GetAllAsync(
		StubServiceNowHandler handler,
		string tableName,
		Options? options = null,
		List<string>? fieldList = null)
	{
		using var client = new ServiceNowClient(handler, options);
		return await client
			.GetAllByQueryAsync(tableName, fieldList: fieldList, cancellationToken: TestContext.Current.CancellationToken)
			.ConfigureAwait(true);
	}

	public static async Task<TException> ThrowsAsync<TException>(
		StubServiceNowHandler handler,
		string tableName,
		Options? options = null) where TException : Exception
	{
		using var client = new ServiceNowClient(handler, options);
		return await Assert
			.ThrowsAnyAsync<TException>(() => client.GetAllByQueryAsync(tableName, cancellationToken: TestContext.Current.CancellationToken))
			.ConfigureAwait(true);
	}
}
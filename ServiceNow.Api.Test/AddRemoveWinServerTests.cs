using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ServiceNow.Api.Test;

public class AddRemoveWinServerTests : ServiceNowTest
{
	public AddRemoveWinServerTests(ILogger<AddRemoveWinServerTests> logger) : base(logger)
	{
	}

	[Fact(Skip = "Cannot run updates on all test systems")]
	public async Task AddUpdateAndDeleteServer_BehavesAsExpected()
	{
		var createDictionary = new Dictionary<string, string> { { "name", "bob" } };
		var newServer = await Client.CreateAsync("cmdb_ci_win_server", JObject.FromObject(createDictionary), System.Threading.CancellationToken.None);
		Assert.NotNull(newServer);
		Assert.Equal("bob", newServer["name"]?.ToString());
		Assert.Equal(string.Empty, newServer["short_description"]?.ToString());

		var sysId = newServer["sys_id"]!.ToString();
		var updateDictionary = new Dictionary<string, string> { { "sys_id", sysId! }, { "short_description", "we updated it" } };
		var updatedServer = await Client.UpdateAsync("cmdb_ci_win_server", JObject.FromObject(updateDictionary));
		Assert.NotNull(updatedServer);
		Assert.Equal(sysId, updatedServer["sys_id"]?.ToString());
		Assert.Equal("bob", updatedServer["name"]?.ToString());
		Assert.Equal("we updated it", updatedServer["short_description"]?.ToString());

		await Client.DeleteAsync("cmdb_ci_win_server", sysId);

		var exception = Record.ExceptionAsync(async () => await Client.GetByIdAsync("cmdb_ci_win_server", sysId));
		Assert.NotNull(exception);
		var message = exception.Result.Message;
		Assert.True(message.StartsWith("Server error NotFound (404): Not Found", System.StringComparison.Ordinal));
	}
}
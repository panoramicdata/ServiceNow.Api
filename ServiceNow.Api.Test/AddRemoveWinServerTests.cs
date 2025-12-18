using AwesomeAssertions;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ServiceNow.Api.Test;

public class AddRemoveWinServerTests(ITestOutputHelper iTestOutputHelper, Fixture fixture) : ServiceNowTest(iTestOutputHelper, fixture)
{
	[Fact(Skip = "Cannot run updates on all test systems")]
	public async Task AddUpdateAndDeleteServer_BehavesAsExpected()
	{
		var createDictionary = new Dictionary<string, string> { { "name", "bob" } };
		var newServer = await Client.CreateAsync("cmdb_ci_win_server", JObject.FromObject(createDictionary), CancellationToken);
		newServer.Should().NotBeNull();
		(newServer["name"]?.ToString()).Should().Be("bob");
		(newServer["short_description"]?.ToString()).Should().Be(string.Empty);

		var sysId = newServer["sys_id"]!.ToString();
		var updateDictionary = new Dictionary<string, string> { { "sys_id", sysId! }, { "short_description", "we updated it" } };
		var updatedServer = await Client.UpdateAsync("cmdb_ci_win_server", JObject.FromObject(updateDictionary), CancellationToken);
		updatedServer.Should().NotBeNull();
		(updatedServer["sys_id"]?.ToString()).Should().Be(sysId);
		(updatedServer["name"]?.ToString()).Should().Be("bob");
		(updatedServer["short_description"]?.ToString()).Should().Be("we updated it");

		await Client.DeleteAsync("cmdb_ci_win_server", sysId, CancellationToken);

		var act = async () => await Client.GetByIdAsync("cmdb_ci_win_server", sysId, CancellationToken);

		await act.Should().ThrowExactlyAsync<System.Exception>()
			.WithMessage("Server error NotFound (404): Not Found*");
	}
}

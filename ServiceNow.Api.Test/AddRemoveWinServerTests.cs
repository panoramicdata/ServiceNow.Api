using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ServiceNow.Api.Test
{
	public class AddRemoveWinServerTests : ServiceNowTest
	{
		public AddRemoveWinServerTests(ITestOutputHelper iTestOutputHelper) : base(iTestOutputHelper)
		{
		}

		[Fact]
		public async Task AddUpdateAndDeleteServer_BehavesAsExpected()
		{
			var createDictionary = new Dictionary<string, string> { { "name", "bob" } };
			var newServer = await Client.CreateAsync("cmdb_ci_win_server", JObject.FromObject(createDictionary)).ConfigureAwait(false);
			Assert.NotNull(newServer);
			Assert.Equal("bob", newServer["name"]?.ToString());
			Assert.Equal(string.Empty, newServer["short_description"]?.ToString());

			var sysId = newServer["sys_id"]!.ToString();
			var updateDictionary = new Dictionary<string, string> { { "sys_id", sysId! }, { "short_description", "we updated it" } };
			var updatedServer = await Client.UpdateAsync("cmdb_ci_win_server", JObject.FromObject(updateDictionary)).ConfigureAwait(false);
			Assert.NotNull(updatedServer);
			Assert.Equal(sysId, updatedServer["sys_id"]?.ToString());
			Assert.Equal("bob", updatedServer["name"]?.ToString());
			Assert.Equal("we updated it", updatedServer["short_description"]?.ToString());

			await Client.DeleteAsync("cmdb_ci_win_server", sysId).ConfigureAwait(false);

			var exception = Record.ExceptionAsync(async () => await Client.GetByIdAsync("cmdb_ci_win_server", sysId).ConfigureAwait(false));
			Assert.NotNull(exception);
			var message = exception.Result.Message;
			Assert.StartsWith("Server error NotFound (404): Not Found", message);
		}
	}
}
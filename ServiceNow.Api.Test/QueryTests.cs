using Microsoft.Extensions.Logging;
using ServiceNow.Api.Tables;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ServiceNow.Api.Test
{
	public class QueryTests : ServiceNowTest
	{
		public QueryTests(ITestOutputHelper output) : base(output, "appsettings.ntt.dev.json", new Options { ValidateCountItemsReturned = true })
		{
		}

		[Fact]
		public async Task InternalPagingTestAsync()
		{
			// This test fails if not ordering by ORDERBYsys_id
			const string query = null;
			var fieldList = new List<string> { "sys_id" };
			const string extraQueryString = null;
			const int pageSize = 10000;

			var result = await Client.GetAllByQueryInternalJObjectAsync("u_ci_property", query, fieldList, extraQueryString, pageSize, default).ConfigureAwait(false);
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.True(result[0].ContainsKey("sys_id"));
			// Not expecting the u_value field to be present as we asked for sys_id only
			Assert.False(result[0].ContainsKey("u_value"));

			// There should only be the properties requested, even though sys_id and sys_created_on are used internally for paging
			var actualProperties = result[0].Properties().Select(p => p.Name).ToList();
			Assert.Equal(fieldList.OrderBy(name => name), actualProperties.OrderBy(name => name));

			// Check for dupes
			var dupes = result.GroupBy(ci => ci["sys_id"]).Where(g => g.Count() > 1).Select(g => new { Id = g.First()["sys_id"], Count = g.Count() }).ToList();

			var unique = result.GroupBy(ci => ci["sys_id"]).Select(ci => ci.First()).ToList();

			Logger.LogInformation($"Found {dupes.Count} dupes - total retrieved = {result.Count} - unique = {unique.Count}");

			Assert.Empty(dupes);
		}

		[Fact]
		public async Task PagingTestAsync()
		{
			// This test fails if not ordering by ORDERBYsys_id
			const string query = null;
			var fieldList = new List<string>();
			const string extraQueryString = null;

			var result = await Client.GetAllByQueryAsync("u_ci_property", query, fieldList, extraQueryString, default).ConfigureAwait(false);
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.True(result[0].ContainsKey("sys_id"));
			// Expecting the u_value field to be present as we didn't limit the fields to be retrieved
			Assert.True(result[0].ContainsKey("u_value"));

			// Check for dupes
			var dupes = result.GroupBy(ci => ci["sys_id"]).Where(g => g.Count() > 1).Select(g => new { Id = g.First()["sys_id"], Count = g.Count() }).ToList();

			var unique = result.GroupBy(ci => ci["sys_id"]).Select(ci => ci.First()).ToList();

			Logger.LogInformation($"Found {dupes.Count} dupes - total retrieved = {result.Count} - unique = {unique.Count}");

			Assert.Empty(dupes);
		}

		[Fact]
		public async void Incident()
		{
			var page = await Client.GetPageByQueryAsync<Incident>(0, 10).ConfigureAwait(false);
			Assert.NotNull(page);
			Assert.NotEmpty(page.Items);
			var firstItem = page.Items[0];

			// Re-fetch using SysId
			var refetchById = await Client.GetByIdAsync<Incident>(firstItem.SysId).ConfigureAwait(false);
			Assert.Equal(firstItem.SysId, refetchById.SysId);

			// Re-fetch using SysId
			var refetchByQuery = (await Client.GetPageByQueryAsync<Incident>(0, 1, $"sys_id={firstItem.SysId}").ConfigureAwait(false))?.Items.FirstOrDefault();
			Assert.NotNull(refetchByQuery);
			Assert.Equal(firstItem.SysId, refetchByQuery.SysId);
		}

		[Fact]
		public async void GetLinkedEntity()
		{
			var server = (await Client.GetAllByQueryAsync("cmdb_ci_win_server", null, new List<string> { "sys_id", "name", "company" }).ConfigureAwait(false)).FirstOrDefault();
			Assert.NotNull(server);
			var companyLink = server["company"]["link"].ToString();
			var company = await Client.GetLinkedEntityAsync(companyLink, new List<string> { "name" }).ConfigureAwait(false);
			Assert.NotNull(company);
			Assert.NotNull(company["name"]);
		}
	}
}
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ServiceNow.Api.Test;

public class GetAllByQueryToleranceTests : ServiceNowTest
{
	public GetAllByQueryToleranceTests(ITestOutputHelper output)
		: base(
			output,
			"appsettings.ntt.dev.json",
			new Options
			{
				ValidateCountItemsReturned = true,
				ValidateCountItemsReturnedTolerance = 5,
				PageSize = 1000
			})
	{
	}

	[Fact]
	public async Task PagingTestAsync()
	{
		// A repeat of another test but with tolerance defined in the client

		// This test fails if not ordering by ORDERBYsys_id
		const string? query = null;
		var fieldList = new List<string>();
		const string? extraQueryString = null;

		var result = await Client.GetAllByQueryAsync("u_ci_property", query, fieldList, extraQueryString, default).ConfigureAwait(false);
		Assert.NotNull(result);
		Assert.NotEmpty(result);
		Assert.True(result[0].ContainsKey("sys_id"));
		// Expecting the u_value field to be present as we didn't limit the fields to be retrieved
		Assert.True(result[0].ContainsKey("u_value"));

		// Check for dupes
		var dupes = result.GroupBy(ci => ci["sys_id"]).Where(g => g.Count() > 1).Select(g => new { Id = g.First()["sys_id"], Count = g.Count() }).ToList();

		var unique = result.GroupBy(ci => ci["sys_id"]).Select(ci => ci.First()).ToList();

		Logger.LogInformation(
			"Found {DupeCount} dupes - total retrieved = {ResultCount} - unique = {UniqueCount}",
			dupes.Count, result.Count, unique.Count);

		Assert.Empty(dupes);
	}


	[Fact]
	public async Task AnotherPagingTestAsync()
	{
		// A repeat of another test but with tolerance defined in the client

		// This test fails if not ordering by ORDERBYsys_id
		const string? query = null;
		var fieldList = new List<string>();
		const string? extraQueryString = null;

		var result = await Client.GetAllByQueryAsync("u_view_websites", query, fieldList, extraQueryString, "wss_created_on", default).ConfigureAwait(false);
		Assert.NotNull(result);
		Assert.NotEmpty(result);
		Assert.True(result[0].ContainsKey("sys_id"));
		// Expecting the u_value field to be present as we didn't limit the fields to be retrieved
		Assert.True(result[0].ContainsKey("u_value"));

		// Check for dupes
		var dupes = result.GroupBy(ci => ci["sys_id"]).Where(g => g.Count() > 1).Select(g => new { Id = g.First()["sys_id"], Count = g.Count() }).ToList();

		var unique = result.GroupBy(ci => ci["sys_id"]).Select(ci => ci.First()).ToList();

		Logger.LogInformation(
			"Found {DupeCount} dupes - total retrieved = {ResultCount} - unique = {UniqueCount}",
			dupes.Count, result.Count, unique.Count);

		Assert.Empty(dupes);
	}
}

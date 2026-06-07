using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ServiceNow.Api.Test;

/// <summary>
/// Tests for the GetAllByQueryAsync method, specifically around tolerance and paging.  These tests are a repeat of some of the tests in ClassTests, but with tolerance defined in the client to ensure that the tolerance logic is working as expected.  These tests should be run against a system with a large number of records in the u_ci_property and u_view_websites tables to ensure that paging is occurring.  Note that these tests will fail if the results are not ordered by ORDERBYsys_id, as this is required for the paging to work correctly with tolerance.  The tests will check for duplicates in the results, which would indicate that the paging is not working correctly.
/// </summary>
/// <param name="iTestOutputHelper"></param>
/// <param name="fixture"></param>
public class GetAllByQueryToleranceTests(ITestOutputHelper iTestOutputHelper, Fixture fixture) : ServiceNowTest(iTestOutputHelper, fixture)
{
	/// <summary>
	/// Tests the GetAllByQueryAsync method with tolerance defined in the client.  This test will fail if the results are not ordered by ORDERBYsys_id, as this is required for the paging to work correctly with tolerance.  The test will check for duplicates in the results, which would indicate that the paging is not working correctly.
	/// </summary>
	/// <returns></returns>
	[Fact(Skip = "This user table does not exist in all test systems")]
	public async Task PagingTestAsync()
	{
		// A repeat of another test but with tolerance defined in the client

		// This test fails if not ordering by ORDERBYsys_id
		const string? query = null;
		var fieldList = new List<string>();
		const string? extraQueryString = null;

		var result = await Client.GetAllByQueryAsync("u_ci_property", query, fieldList, extraQueryString, cancellationToken: CancellationToken);
		result.Should().NotBeNull();
		Assert.NotEmpty(result);
		result[0].ContainsKey("sys_id").Should().BeTrue();
		// Expecting the u_value field to be present as we didn't limit the fields to be retrieved
		result[0].ContainsKey("u_value").Should().BeTrue();

		// Check for dupes
		var dupes = result.GroupBy(ci => ci["sys_id"]).Where(g => g.Count() > 1).Select(g => new { Id = g.First()["sys_id"], Count = g.Count() }).ToList();

		var unique = result.GroupBy(ci => ci["sys_id"]).Select(ci => ci.First()).ToList();

		Logger.LogInformation(
			"Found {DupeCount} dupes - total retrieved = {ResultCount} - unique = {UniqueCount}",
			dupes.Count, result.Count, unique.Count);

		dupes.Should().BeEmpty();
	}

	/// <summary>
	/// Another test of the GetAllByQueryAsync method with tolerance defined in the client, but against a different table.  This test will also fail if the results are not ordered by ORDERBYsys_id, as this is required for the paging to work correctly with tolerance.  The test will check for duplicates in the results, which would indicate that the paging is not working correctly.
	/// </summary>
	[Fact(Skip = "This user table does not exist in all test systems")]
	public async Task AnotherPagingTestAsync()
	{
		// A repeat of another test but with tolerance defined in the client

		// This test fails if not ordering by ORDERBYsys_id
		const string? query = null;
		var fieldList = new List<string>();
		const string? extraQueryString = null;

		var result = await Client.GetAllByQueryAsync(
			"u_view_websites",
			query,
			fieldList,
			extraQueryString,
			"wss_sys_updated_on",
			3,
			CancellationToken);
		result.Should().NotBeNull();
		Assert.NotEmpty(result);
		result[0].ContainsKey("sys_id").Should().BeTrue();
		// Expecting the u_value field to be present as we didn't limit the fields to be retrieved
		result[0].ContainsKey("u_value").Should().BeTrue();

		// Check for dupes
		var dupes = result.GroupBy(ci => ci["sys_id"]).Where(g => g.Count() > 1).Select(g => new { Id = g.First()["sys_id"], Count = g.Count() }).ToList();

		var unique = result.GroupBy(ci => ci["sys_id"]).Select(ci => ci.First()).ToList();

		Logger.LogInformation(
			"Found {DupeCount} dupes - total retrieved = {ResultCount} - unique = {UniqueCount}",
			dupes.Count, result.Count, unique.Count);

		dupes.Should().BeEmpty();
	}
}

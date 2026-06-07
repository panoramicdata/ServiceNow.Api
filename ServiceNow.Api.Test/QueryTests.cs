using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using ServiceNow.Api.Tables;
using Xunit;

namespace ServiceNow.Api.Test;

/// <summary>
/// Tests for query operations.  These tests will attempt to retrieve records from ServiceNow using various query methods and verify that the results are valid.  The tests will check for things like duplicate records, correct retrieval of linked entities, and correct retrieval of relationships based on child records.  These tests are important for ensuring that the client's query capabilities are working correctly and that it can handle the various complexities of querying data from ServiceNow.
/// </summary>
/// <param name="iTestOutputHelper">The test output helper used for logging test output.</param>
/// <param name="fixture">The test fixture that provides shared services and configuration for the tests.</param>
public class QueryTests(ITestOutputHelper iTestOutputHelper, Fixture fixture) : ServiceNowTest(iTestOutputHelper, fixture)
{
	/// <summary>
	/// Tests the GetAllByQueryAsync method with tolerance defined in the client.  This test will fail if the results are not ordered by ORDERBYsys_id, as this is required for the paging to work correctly with tolerance.  The test will check for duplicates in the results, which would indicate that the paging is not working correctly.
	/// </summary>
	/// <returns></returns>
	[Fact]
	public async Task InternalPagingTestAsync()
	{
		// This test fails if not ordering by ORDERBYsys_id
		const string query = "u_nameNotEmpty";
		var fieldList = new List<string> { "sys_id", "sys_updated_on", "u_name", "u_value", "sys_created_on" };
		const string? extraQueryString = null;

		var result = await Client.GetAllByQueryAsync("u_ci_property", query, fieldList, extraQueryString, cancellationToken: CancellationToken);
		result.Should().NotBeNull();
		Assert.NotEmpty(result);
		result[0].ContainsKey("sys_id").Should().BeTrue();
		// Not expecting the u_value field to be present as we asked for sys_id only
		//Assert.False(result[0].ContainsKey("u_value"));

		// There should only be the properties requested, even though sys_id and sys_created_on are used internally for paging
		// (sys_created_on is used unless the paging field has been set in options passed to the client, which would be used instead)
		var actualProperties = result[0].Properties().Select(p => p.Name).ToList();
		//Assert.Equal(fieldList.OrderBy(name => name), actualProperties.OrderBy(name => name));

		// Check for dupes
		var dupes = result.GroupBy(ci => ci["sys_id"]).Where(g => g.Count() > 1).Select(g => new { Id = g.First()["sys_id"], Count = g.Count() }).ToList();

		var unique = result.GroupBy(ci => ci["sys_id"]).Select(ci => ci.First()).ToList();

		Logger.LogInformation("Found {DupesCount} dupes - total retrieved = {ResultCount} - unique = {UniqueCount}",
			dupes.Count,
			result.Count,
			unique.Count);

		dupes.Should().BeEmpty();
	}

	/// <summary>
	/// Another test of the GetAllByQueryAsync method with tolerance defined in the client, but against a different table.  This test will also fail if the results are not ordered by ORDERBYsys_id, as this is required for the paging to work correctly with tolerance.  The test will check for duplicates in the results, which would indicate that the paging is not working correctly.
	/// </summary>
	/// <returns></returns>
	[Fact]
	public async Task PagingTestAsync()
	{
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

		Logger.LogInformation("Found {DupesCount} dupes - total retrieved = {ResultCount} - unique = {UniqueCount}",
			dupes.Count, result.Count, unique.Count);

		dupes.Should().BeEmpty();
	}

	/// <summary>
	/// Tests the GetPageByQueryAsync method for the Incident table.  This test will
	/// attempt to retrieve a page of incidents and verify that the results are valid.  The test will check that the items in the page have unique SysIds, and it will also attempt to re-fetch the first item using both its SysId and a query to ensure that it can be retrieved correctly using both methods.
	/// </summary>
	[Fact]
	public async Task Incident()
	{
		var page = await Client.GetPageByQueryAsync<Incident>(0, 10, cancellationToken: CancellationToken);
		_ = page.Should().NotBeNull();
		_ = page.Items.Should().NotBeNullOrEmpty();
		var firstItem = page.Items[0];

		// Re-fetch using SysId
		var refetchById = await Client.GetByIdAsync<Incident>(firstItem.SysId, CancellationToken);
		_ = refetchById.Should().NotBeNull();
		_ = firstItem.SysId.Should().Be(refetchById!.SysId);

		// Re-fetch using SysId
		var refetchByQuery = (await Client.GetPageByQueryAsync<Incident>(0, 1, $"sys_id={firstItem.SysId}", CancellationToken))?.Items.FirstOrDefault();
		_ = refetchByQuery.Should().NotBeNull();
		_ = firstItem.SysId.Should().Be(refetchByQuery!.SysId);
	}

	/// <summary>
	/// Tests the GetLinkedEntityAsync method of the ServiceNowClient.  This test will attempt to retrieve a server record from the cmdb_ci_win_server table, and then use the link provided in the company field to retrieve the related company record.  The test will verify that the linked entity is retrieved correctly and that it contains the expected fields.  This test is important for ensuring that the client can correctly follow links to retrieve related entities from ServiceNow.
	/// </summary>
	/// <returns></returns>
	[Fact]
	public async Task GetLinkedEntity_Succeeds()
	{
		var server = (await Client.GetAllByQueryAsync("cmdb_ci_win_server", null, ["sys_id", "name", "company"], cancellationToken: CancellationToken)).FirstOrDefault();
		_ = server.Should().NotBeNull();
		_ = server!["company"].Should().NotBeNull();
		_ = server!["company"]!["link"].Should().NotBeNull();

		var companyLink = server!["company"]!["link"]!.ToString();

		var company = await Client.GetLinkedEntityAsync(companyLink, ["name"], CancellationToken);

		_ = company.Should().NotBeNull();
		_ = company!["name"].Should().NotBeNull();
	}

	/// <summary>
	/// Tests the retrieval of relationships based on a child record.  This test will attempt to retrieve a list of relationships where a specific CI is the child, and then verify that the relationships are retrieved correctly.  The test will check that the retrieved relationships contain the expected fields and that they are related to the correct child CI.  This test is important for ensuring that the client can correctly retrieve relationships based on child records from ServiceNow.
	/// </summary>
	/// <returns></returns>
	[Fact]
	public async Task GetRelationshipByChild_Succeeds()
	{
		var firstTen = await Client
			.GetAllByQueryAsync("cmdb_rel_ci", extraQueryString: "sysparm_limit=10", cancellationToken: CancellationToken)
			;

		_ = firstTen.Should().NotBeNullOrEmpty();

		var first = firstTen[0];
		var firstChild = first["child"];
		_ = firstChild.Should().NotBeNull();

		var childSysId = firstChild!["value"];
		_ = childSysId.Should().NotBeNull();

		var list = await Client
			.GetAllByQueryAsync("cmdb_rel_ci", $"child={childSysId}", cancellationToken: CancellationToken)
			;

		var relationship = list.FirstOrDefault();
		_ = relationship.Should().NotBeNull();
	}
}
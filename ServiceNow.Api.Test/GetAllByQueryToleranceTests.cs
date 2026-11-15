using AwesomeAssertions;
using Newtonsoft.Json.Linq;
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
		var result = await Client.GetAllByQueryAsync("u_ci_property", null, new List<string>(), null, cancellationToken: CancellationToken);
		ValidatePagingResult(result);
	}

	/// <summary>
	/// Another test of the GetAllByQueryAsync method with tolerance defined in the client, but against a different table.  This test will also fail if the results are not ordered by ORDERBYsys_id, as this is required for the paging to work correctly with tolerance.  The test will check for duplicates in the results, which would indicate that the paging is not working correctly.
	/// </summary>
	[Fact(Skip = "This user table does not exist in all test systems")]
	public async Task AnotherPagingTestAsync()
	{
		// A repeat of another test but with tolerance defined in the client
		// This test fails if not ordering by ORDERBYsys_id
		var result = await Client.GetAllByQueryAsync(
			"u_view_websites",
			null,
			new List<string>(),
			null,
			"wss_sys_updated_on",
			3,
			CancellationToken);
		ValidatePagingResult(result);
	}

	private void ValidatePagingResult(List<JObject> result)
	{
		Assert.NotEmpty(result);
		result[0].ContainsKey("sys_id").Should().BeTrue();
		// Expecting the u_value field to be present as we didn't limit the fields to be retrieved
		result[0].ContainsKey("u_value").Should().BeTrue();

		AssertNoDuplicateSysIds(result);
	}
}

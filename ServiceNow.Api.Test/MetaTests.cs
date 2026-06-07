using AwesomeAssertions;
using Xunit;

namespace ServiceNow.Api.Test;

/// <summary>
/// Tests for metadata retrieval operations.  These tests will attempt to retrieve metadata for a specific class and verify that the results are valid.  The tests will check that the parent class is correct, that there are attributes defined, and that there are child classes defined.  These tests are important for ensuring that the client can correctly retrieve and interpret metadata from the ServiceNow API, which is essential for many operations that depend on understanding the structure of the data in ServiceNow.
/// </summary>
/// <param name="iTestOutputHelper">The test output helper used for logging test output.</param>
/// <param name="fixture">The test fixture that provides shared services and configuration for the tests.</param>
public class MetaTests(ITestOutputHelper iTestOutputHelper, Fixture fixture) : ServiceNowTest(iTestOutputHelper, fixture)
{
	/// <summary>
	/// Tests the GetMetaForClassAsync method of the ServiceNowClient.  This test will attempt to retrieve metadata for the "cmdb_ci_server" class and verify that the results are valid.  The test checks that the parent class is "cmdb_ci_computer", that there are attributes defined, and that there are child classes defined.  If any of these conditions are not met, the test will fail.  This test is important for ensuring that the client can correctly retrieve and interpret metadata from the ServiceNow API, which is essential for many operations that depend on understanding the structure of the data in ServiceNow.
	/// </summary>
	/// <returns></returns>
	[Fact]
	public async Task MetaData_Get_ValidResult()
	{
		const string className = "cmdb_ci_server";
		var result = await Client.GetMetaForClassAsync(className, CancellationToken);
		result.Should().NotBeNull();
		result.Item.Should().NotBeNull();
		result.Item!.Parent.Should().Be("cmdb_ci_computer");
		result.Item.Attributes.Should().NotBeNull();
		Assert.NotEmpty(result.Item.Attributes);
		result.Item.Children.Should().NotBeNull();
		Assert.NotEmpty(result.Item.Children);
	}
}
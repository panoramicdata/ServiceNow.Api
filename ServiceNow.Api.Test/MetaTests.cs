using AwesomeAssertions;
using System.Threading.Tasks;
using Xunit;

namespace ServiceNow.Api.Test;

public class MetaTests(ITestOutputHelper iTestOutputHelper, Fixture fixture) : ServiceNowTest(iTestOutputHelper, fixture)
{
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
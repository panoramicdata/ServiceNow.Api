using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ServiceNow.Api.Test;

public class MetaTests(ITestOutputHelper iTestOutputHelper, Fixture fixture) : ServiceNowTest(iTestOutputHelper, fixture)
{
	[Fact]
	public async Task MetaData_Get_ValidResult()
	{
		const string className = "cmdb_ci_server";
		var result = await Client.GetMetaForClassAsync(className);
		Assert.NotNull(result);
		Assert.NotNull(result.Item);
		Assert.Equal("cmdb_ci_computer", result.Item!.Parent);
		Assert.NotNull(result.Item.Attributes);
		Assert.NotEmpty(result.Item.Attributes);
		Assert.NotNull(result.Item.Children);
		Assert.NotEmpty(result.Item.Children);
	}
}
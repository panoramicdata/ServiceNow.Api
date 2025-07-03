using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Xunit;

namespace ServiceNow.Api.Test;

public class MetaTests : ServiceNowTest
{
	public MetaTests(ILogger<MetaTests> logger) : base(logger)
	{
	}

	[Fact]
	public async Task MetaData_Get_ValidResult()
	{
		const string className = "cmdb_ci_server";
		var result = await Client.GetMetaForClassAsync(className);
		Assert.IsNotNull(result);
		Assert.IsNotNull(result.Item);
		Assert.AreEqual("cmdb_ci_computer", result.Item!.Parent);
		Assert.IsNotNull(result.Item.Attributes);
		Assert.IsNotEmpty(result.Item.Attributes);
		Assert.IsNotNull(result.Item.Children);
		Assert.IsNotEmpty(result.Item.Children);
	}
}
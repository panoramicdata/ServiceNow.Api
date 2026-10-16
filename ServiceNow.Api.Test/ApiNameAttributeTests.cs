using AwesomeAssertions;
using ServiceNow.Api.Attributes;
using Xunit;

namespace ServiceNow.Api.Test;

public class ApiNameAttributeTests
{
	[Theory]
	[InlineData("cmdb_ci_server")]
	[InlineData("sc_request")]
	[InlineData("")]
	public void Constructor_SetsApiName(string apiName)
	{
		var attribute = new ApiNameAttribute(apiName);

		attribute.ApiName.Should().Be(apiName);
	}
}

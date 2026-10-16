using AwesomeAssertions;
using ServiceNow.Api;
using ServiceNow.Api.Tables;
using Xunit;

namespace ServiceNow.Api.Test;

public class CmdbEntityTests
{
	[Fact]
	public void SysId_RoundTrip()
	{
		var server = new WindowsServer { SysId = "abc123" };

		server.SysId.Should().Be("abc123");
	}

	[Fact]
	public void GetApiName_Table_Type_ReturnsApiName()
	{
		CmdbEntity.GetApiName<Server>().Should().Be("table");
	}
}

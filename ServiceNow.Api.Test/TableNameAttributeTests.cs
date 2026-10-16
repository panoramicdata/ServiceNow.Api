using AwesomeAssertions;
using ServiceNow.Api.Attributes;
using Xunit;

namespace ServiceNow.Api.Test;

public class TableNameAttributeTests
{
	[Theory]
	[InlineData("cmdb_ci")]
	[InlineData("sys_user")]
	[InlineData("")]
	public void Constructor_SetsTableName(string tableName)
	{
		var attribute = new TableNameAttribute(tableName);

		attribute.TableName.Should().Be(tableName);
	}
}

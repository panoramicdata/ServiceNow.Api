using AwesomeAssertions;
using ServiceNow.Api;
using ServiceNow.Api.Tables;
using Xunit;

namespace ServiceNow.Api.Test;

public class ResourceLinkTests
{
	[Theory]
	[InlineData("https://test.service-now.com", "abc123")]
	[InlineData(null, "def456")]
	[InlineData("https://example.com", null)]
	[InlineData(null, null)]
	public void Properties_RoundTrip(string? link, string? value)
	{
		var resourceLink = new ResourceLink<WindowsServer>
		{
			Link = link,
			Value = value,
		};

		resourceLink.Link.Should().Be(link);
		resourceLink.Value.Should().Be(value);
	}

	[Theory]
	[InlineData("abc123", "abc123")]
	[InlineData(null, "")]
	public void ToString_ReturnsValueOrEmpty(string? value, string expected)
	{
		var resourceLink = new ResourceLink<WindowsServer> { Value = value };

		resourceLink.ToString().Should().Be(expected);
	}
}

using AwesomeAssertions;
using Newtonsoft.Json;
using ServiceNow.Api.Tables;
using Xunit;
using Xunit.Abstractions;

namespace ServiceNow.Api.Test.Converters;

public class ResourceLinkConverterTests(ITestOutputHelper iTestOutputHelper, Fixture fixture) : ServiceNowTest(iTestOutputHelper, fixture)
{
	[Fact]
	public void CanConvert_ApplicationLink_ConfirmsResponsibility()
	{
		var converter = new Api.Converters.ResourceLinkConverter();
		var result = converter.CanConvert(typeof(ResourceLink<Application>));

		result.Should().BeTrue();
	}

	[Fact]
	public void ReadJson_StandardLinkJson_DeserializesToValidObject()
	{
		var json = "{\"link\" : \"https://test.service-now.com\", \"value\" : \"Test\"}";
		var resourceLink = JsonConvert.DeserializeObject<ResourceLink<Application>>(json);

		resourceLink.Should().NotBeNull();
		resourceLink!.Link.Should().Be("https://test.service-now.com");
		resourceLink!.Value.Should().Be("Test");
	}

	[Fact]
	public void WriteJson_ApplicationResourceLink_GeneratesCorrectString()
	{
		var resourceLink = new ResourceLink<Application>
		{
			Link = "https://test.service-now.com",
			Value = "Test"
		};
		var settings = new JsonSerializerSettings { Formatting = Formatting.None };
		var json = JsonConvert.SerializeObject(resourceLink, settings);

		json.Should().Be("\"Test\"");
	}
}

using AwesomeAssertions;
using Newtonsoft.Json.Linq;
using System.Net;
using Xunit;

namespace ServiceNow.Api.Test;

public class NullResultTests
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	private sealed class StubHandler(HttpStatusCode status, string body) : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			=> Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body) });
	}

	private static ServiceNowClient CreateClient(string body, HttpStatusCode status = HttpStatusCode.OK)
		=> new(new StubHandler(status, body), new Options());

	[Theory]
	[InlineData("")]
	[InlineData("null")]
	[InlineData("{}")]
	[InlineData("{\"result\":null}")]
	public async Task GetByIdAsync_ReturnsNull_ForEmptyOrNullBody(string body)
	{
		using var client = CreateClient(body);

		var result = await client.GetByIdAsync("incident", "abc", Ct);

		result.Should().BeNull();
	}

	[Theory]
	[InlineData("")]
	[InlineData("{}")]
	[InlineData("{\"result\":null}")]
	public async Task GetLinkedEntityAsync_ReturnsNull_ForEmptyOrNullBody(string body)
	{
		using var client = CreateClient(body);

		var result = await client.GetLinkedEntityAsync("https://x.service-now.com/api/now/table/incident/abc", [], Ct);

		result.Should().BeNull();
	}

	[Theory]
	[InlineData("")]
	[InlineData("{}")]
	[InlineData("{\"result\":null}")]
	public async Task GetAttachmentsAsync_ReturnsEmpty_ForEmptyOrNullBody(string body)
	{
		using var client = CreateClient(body);

		var result = await client.GetAttachmentsAsync("incident", "abc", Ct);

		result.Should().BeEmpty();
	}

	[Theory]
	[InlineData("", HttpStatusCode.OK)]
	[InlineData("{}", HttpStatusCode.Created)]
	[InlineData("{\"result\":null}", HttpStatusCode.Created)]
	public async Task CreateAsync_DoesNotThrow_ForEmptyOrNullBody(string body, HttpStatusCode status)
	{
		using var client = CreateClient(body, status);

		var act = () => client.CreateAsync("incident", new JObject { ["short_description"] = "x" }, Ct);

		await act.Should().NotThrowAsync();
	}

	[Theory]
	[InlineData("")]
	[InlineData("{}")]
	[InlineData("{\"result\":null}")]
	public async Task GetMetaForClassAsync_DoesNotThrow_ForEmptyOrNullBody(string body)
	{
		using var client = CreateClient(body);

		var result = await client.GetMetaForClassAsync("cmdb_ci", Ct);

		result.Should().NotBeNull();
		result.Item.Should().BeNull();
	}
}

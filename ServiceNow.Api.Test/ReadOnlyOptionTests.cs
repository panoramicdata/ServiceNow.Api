using AwesomeAssertions;
using Newtonsoft.Json.Linq;
using System.Net;
using Xunit;

namespace ServiceNow.Api.Test;

/// <summary>
/// Credential-free: a refused write throws before any request reaches the handler.
/// </summary>
public class ReadOnlyOptionTests
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	private sealed class RecordingHandler : HttpMessageHandler
	{
		public List<HttpMethod> Methods { get; } = [];

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			Methods.Add(request.Method);
			return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent("{\"result\":{\"sys_id\":\"abc\"}}")
			});
		}
	}

	private static ServiceNowClient CreateClient(RecordingHandler handler, bool isReadOnly)
		=> new(handler, new Options { IsReadOnly = isReadOnly });

	[Fact]
	public void IsReadOnly_DefaultsToFalse()
		=> new Options().IsReadOnly.Should().BeFalse();

	[Fact]
	public async Task Create_IsRefused_WhenIsReadOnly()
	{
		using var handler = new RecordingHandler();
		using var client = CreateClient(handler, isReadOnly: true);

		var act = () => client.CreateAsync("incident", new JObject { ["short_description"] = "x" }, Ct);

		await act.Should().ThrowAsync<InvalidOperationException>();
		handler.Methods.Should().BeEmpty();
	}

	[Fact]
	public async Task Update_IsRefused_WhenIsReadOnly()
	{
		using var handler = new RecordingHandler();
		using var client = CreateClient(handler, isReadOnly: true);

		var act = () => client.UpdateAsync("incident", new JObject { ["sys_id"] = "abc" }, Ct);

		await act.Should().ThrowAsync<InvalidOperationException>();
		handler.Methods.Should().BeEmpty();
	}

	[Fact]
	public async Task Patch_IsRefused_WhenIsReadOnly()
	{
		using var handler = new RecordingHandler();
		using var client = CreateClient(handler, isReadOnly: true);

		var act = () => client.PatchAsync("incident", new JObject { ["sys_id"] = "abc" }, Ct);

		await act.Should().ThrowAsync<InvalidOperationException>();
		handler.Methods.Should().BeEmpty();
	}

	[Fact]
	public async Task Delete_IsRefused_WhenIsReadOnly()
	{
		using var handler = new RecordingHandler();
		using var client = CreateClient(handler, isReadOnly: true);

		var act = () => client.DeleteAsync("incident", "abc", Ct);

		await act.Should().ThrowAsync<InvalidOperationException>();
		handler.Methods.Should().BeEmpty();
	}

	[Fact]
	public async Task Delete_IsAllowed_WhenNotReadOnly()
	{
		using var handler = new RecordingHandler();
		using var client = CreateClient(handler, isReadOnly: false);

		await client.DeleteAsync("incident", "abc", Ct);

		handler.Methods.Should().ContainSingle().Which.Should().Be(HttpMethod.Delete);
	}

	[Fact]
	public async Task Get_IsAllowed_WhenIsReadOnly()
	{
		using var handler = new RecordingHandler();
		using var client = CreateClient(handler, isReadOnly: true);

		var result = await client.GetByIdAsync("incident", "abc", Ct);

		result.Should().NotBeNull();
		handler.Methods.Should().ContainSingle().Which.Should().Be(HttpMethod.Get);
	}
}

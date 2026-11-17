using AwesomeAssertions;
using System.Net;
using Xunit;

namespace ServiceNow.Api.Test;

/// <summary>
/// Credential-free: the public constructor is exercised with an injected handler, so no network is used.
/// </summary>
public class ClientOptionsTests
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	private sealed class RecordingHandler : HttpMessageHandler
	{
		public List<Uri?> Uris { get; } = [];

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			Uris.Add(request.RequestUri);
			return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent("{\"result\":{\"sys_id\":\"abc\"}}")
			});
		}
	}

	private sealed class HangingHandler : HttpMessageHandler
	{
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(request);
			await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
			return new HttpResponseMessage(HttpStatusCode.OK);
		}
	}

	[Fact]
	public void Timeout_AndHandler_DefaultToNull()
	{
		var options = new Options();
		options.Timeout.Should().BeNull();
		options.HttpMessageHandler.Should().BeNull();
	}

	[Fact]
	public async Task InjectedHandler_IsUsed_ByPublicConstructor()
	{
		var handler = new RecordingHandler();
		using var client = new ServiceNowClient("mycompany", "user", "pass", new Options { HttpMessageHandler = handler });

		var result = await client.GetByIdAsync("incident", "abc", Ct);

		result.Should().NotBeNull();
		handler.Uris.Should().ContainSingle().Which!.Host.Should().Be("mycompany.service-now.com");
	}

	[Fact]
	public async Task Timeout_IsHonoured()
	{
		using var client = new ServiceNowClient(
			"mycompany",
			"user",
			"pass",
			new Options { HttpMessageHandler = new HangingHandler(), Timeout = TimeSpan.FromMilliseconds(100) });

		var act = () => client.GetByIdAsync("incident", "abc", Ct);

		await act.Should().ThrowAsync<TaskCanceledException>();
	}
}

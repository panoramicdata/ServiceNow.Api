using System.Net;

namespace ServiceNow.Api.Test;

/// <summary>
/// A message handler that returns one fixed response for every request.
/// </summary>
internal sealed class StubHandler(HttpStatusCode status, string body) : HttpMessageHandler
{
	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body), RequestMessage = request });
	}
}
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net;
using System.Text;
using System.Web;

namespace ServiceNow.Api.Test;

/// <summary>
/// Serves a fixed sequence of pages and reports a fixed X-Total-Count, so that paging behaviour can be
/// exercised deterministically without a live ServiceNow instance. The query is ignored when choosing what
/// to return: these tests are about how responses are interpreted, not about query construction. The
/// requested URLs are recorded so that a test can assert on what the client asked for.
/// </summary>
internal sealed class StubServiceNowHandler(int totalCount, IReadOnlyList<List<JObject>> pages) : HttpMessageHandler
{
	private readonly List<string> _requestUris = [];

	public int RequestCount => _requestUris.Count;

	/// <summary>
	/// The path and query of every request made, in order.
	/// </summary>
	public IReadOnlyList<string> RequestUris => _requestUris;

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var index = _requestUris.Count;

		// HttpUtility.UrlDecode is the exact inverse of the HttpUtility.UrlEncode the client uses, so a
		// space encoded as '+' comes back as a space. Uri.UnescapeDataString would leave it as '+'.
		_requestUris.Add(HttpUtility.UrlDecode(request.RequestUri?.PathAndQuery ?? string.Empty));

		var rows = index < pages.Count ? pages[index] : [];
		var payload = new JObject { ["result"] = new JArray(rows) };

		var response = new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(payload.ToString(), Encoding.UTF8, "application/json")
		};
		response.Headers.Add("X-Total-Count", totalCount.ToString(CultureInfo.InvariantCulture));

		return Task.FromResult(response);
	}
}

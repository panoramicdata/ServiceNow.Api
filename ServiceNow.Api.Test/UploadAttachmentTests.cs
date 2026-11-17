using AwesomeAssertions;
using System.Net;
using System.Text;
using Xunit;

namespace ServiceNow.Api.Test;

public class UploadAttachmentTests
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	private sealed class CapturingHandler : HttpMessageHandler
	{
		public HttpMethod? Method { get; private set; }
		public string? PathAndQuery { get; private set; }
		public string? ContentType { get; private set; }
		public string? Body { get; private set; }

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			Method = request.Method;
			PathAndQuery = request.RequestUri?.PathAndQuery;
			ContentType = request.Content?.Headers.ContentType?.ToString();
			Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
			return new HttpResponseMessage(HttpStatusCode.Created)
			{
				Content = new StringContent("{\"result\":{\"sys_id\":\"att1\",\"file_name\":\"a b.txt\",\"table_name\":\"incident\",\"table_sys_id\":\"inc1\"}}")
			};
		}
	}

	[Fact]
	public async Task UploadAttachmentAsync_PostsRawBodyWithQueryParameters()
	{
		var handler = new CapturingHandler();
		using var client = new ServiceNowClient(handler, new Options());
		using var content = new MemoryStream(Encoding.UTF8.GetBytes("hello"));

		var result = await client.UploadAttachmentAsync("incident", "inc1", "a b.txt", content, "text/plain", Ct);

		handler.Method.Should().Be(HttpMethod.Post);
		handler.PathAndQuery.Should().Be("/api/now/attachment/file?table_name=incident&table_sys_id=inc1&file_name=a+b.txt");
		handler.ContentType.Should().Be("text/plain");
		handler.Body.Should().Be("hello");
		result.Should().NotBeNull();
		result!.SysId.Should().Be("att1");
		result.FileName.Should().Be("a b.txt");
		result.TableSysId.Should().Be("inc1");
	}

	[Fact]
	public async Task UploadAttachmentAsync_DefaultsContentTypeToOctetStream()
	{
		var handler = new CapturingHandler();
		using var client = new ServiceNowClient(handler, new Options());
		using var content = new MemoryStream([1, 2, 3]);

		await client.UploadAttachmentAsync("incident", "inc1", "a.bin", content, cancellationToken: Ct);

		handler.ContentType.Should().Be("application/octet-stream");
	}

	[Fact]
	public async Task UploadAttachmentAsync_IsRefused_WhenIsReadOnly()
	{
		var handler = new CapturingHandler();
		using var client = new ServiceNowClient(handler, new Options { IsReadOnly = true });
		using var content = new MemoryStream([1]);

		var act = () => client.UploadAttachmentAsync("incident", "inc1", "a.bin", content, cancellationToken: Ct);

		await act.Should().ThrowAsync<InvalidOperationException>();
		handler.Method.Should().BeNull();
	}
}

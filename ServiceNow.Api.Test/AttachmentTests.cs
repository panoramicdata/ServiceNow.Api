using AwesomeAssertions;
using ServiceNow.Api.Tables;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace ServiceNow.Api.Test;

public class AttachmentTests(ITestOutputHelper iTestOutputHelper, Fixture fixture) : ServiceNowTest(iTestOutputHelper, fixture)
{
	[Fact(Skip = "Needs a request with an attachment")]
	public async Task GetAllAttachments()
	{
		// Go and get 10 items for the type we're testing
		var request = await Client.GetByIdAsync<Request>("2c8e0e3b4f20130003d03b718110c762", CancellationToken);
		// Make sure that something was returned
		request.Should().NotBeNull();
		var attachments = await Client.GetAttachmentsAsync(request!, CancellationToken);
		attachments.Should().NotBeNull();
		Assert.Single(attachments);
		var localPath = await Client.DownloadAttachmentAsync(attachments[0], Path.GetTempPath(), cancellationToken: CancellationToken);
		localPath.Should().NotBeNull();
	}
}

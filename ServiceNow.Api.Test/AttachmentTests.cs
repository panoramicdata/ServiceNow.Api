using Microsoft.Extensions.Logging;
using ServiceNow.Api.Tables;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace ServiceNow.Api.Test;

public class AttachmentTests : ServiceNowTest
{
	public AttachmentTests(ILogger<AddRemoveWinServerTests> logger) : base(logger)
	{
	}

#pragma warning disable xUnit1004 // Test methods should not be skipped
	[Fact(Skip = "Needs a request with an attachment")]
#pragma warning restore xUnit1004 // Test methods should not be skipped
	public async Task GetAllAttachments()
	{
		// Go and get 10 items for the type we're testing
		var request = await Client.GetByIdAsync<Request>("2c8e0e3b4f20130003d03b718110c762");
		// Make sure that something was returned
		Assert.NotNull(request);
		var attachments = await Client.GetAttachmentsAsync(request!);
		Assert.NotNull(attachments);
		Assert.Equal(1, attachments.Count);
		var localPath = await Client.DownloadAttachmentAsync(attachments[0], Path.GetTempPath());
		Assert.NotNull(localPath);
	}
}
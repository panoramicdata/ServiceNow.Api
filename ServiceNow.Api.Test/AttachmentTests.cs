using ServiceNow.Api.Tables;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ServiceNow.Api.Test
{
	public class AttachmentTests : ServiceNowTest
	{
		public AttachmentTests(ITestOutputHelper iTestOutputHelper) : base(iTestOutputHelper)
		{
		}

#pragma warning disable xUnit1004 // Test methods should not be skipped
		[Fact(Skip = "Needs a request with an attachment")]
#pragma warning restore xUnit1004 // Test methods should not be skipped
		public async void GetAllAttachments()
		{
			// Go and get 10 items for the type we're testing
			var request = await Client.GetByIdAsync<Request>("2c8e0e3b4f20130003d03b718110c762").ConfigureAwait(false);
			// Make sure that something was returned
			Assert.NotNull(request);
			var attachments = await Client.GetAttachmentsAsync(request).ConfigureAwait(false);
			Assert.NotNull(attachments);
			Assert.Single(attachments);
			var localPath = await Client.DownloadAttachmentAsync(attachments[0], Path.GetTempPath()).ConfigureAwait(false);
			Assert.NotNull(localPath);
		}

		[Fact]
		public async void GetAttachmentAsStream_UploadAttachment()
		{
			string attachmentSysid = "40aa5a1bdb33c810916aee805b9619ca";
			string tableName = "u_xxx_request";

			var request = await Client.GetByIdAsync<Request>(attachmentSysid).ConfigureAwait(true);
			var attachments = await Client.GetAttachmentsAsync(tableName, attachmentSysid).ConfigureAwait(true);

			string filePath = string.Empty;
			string filename = string.Empty;
			using (var stream = await Client.DownloadAttachmentAsyncAsStream(attachments[0], Path.GetTempPath()).ConfigureAwait(true))
			{
				filename = attachments[0].FileName;
				filePath = Path.GetTempPath() + filename;
				Stream streamToWriteTo = File.Open(filePath, FileMode.Create);
				await stream.CopyToAsync(streamToWriteTo).ConfigureAwait(false);
				streamToWriteTo.Close();
			}

			MemoryStream inMemoryCopy = new MemoryStream();
			using (FileStream fs = File.OpenRead(filePath))
			{
				fs.CopyTo(inMemoryCopy);
			}
			byte[] bs = inMemoryCopy.ToArray();

			bool response = await Client.UploadAttachmentAsync(filename, tableName, attachmentSysid, bs);
		}
	}
}
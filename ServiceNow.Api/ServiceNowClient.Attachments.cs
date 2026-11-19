using ServiceNow.Api.Tables;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Web;

namespace ServiceNow.Api;

public partial class ServiceNowClient
{
	/// <summary>
	/// Get attachments for a given Table based entry
	/// </summary>
	/// <typeparam name="T">The type of object</typeparam>
	/// <param name="table">The object itself</param>
	/// <returns>A list of attachments</returns>
	public Task<List<Attachment>> GetAttachmentsAsync<T>(T table) where T : Table
		=> GetAttachmentsAsync(table, CancellationToken.None);

	/// <summary>
	/// Get attachments for a given Table based entry
	/// </summary>
	/// <typeparam name="T">The type of object</typeparam>
	/// <param name="table">The object itself</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>A list of attachments</returns>
	public Task<List<Attachment>> GetAttachmentsAsync<T>(T table, CancellationToken cancellationToken) where T : Table
		=> GetAttachmentsAsync(Table.GetTableName<T>(), table.SysId, cancellationToken);

	/// <summary>
	/// Get attachments for a given Table based entry
	/// </summary>
	/// <param name="tableName">The name of the table</param>
	/// <param name="tableSysId">The sys_id of the entry in the referenced table</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>A list of attachments</returns>
	public async Task<List<Attachment>> GetAttachmentsAsync(
		string tableName,
		string tableSysId,
		[Optional] CancellationToken cancellationToken)
		=> (await GetInternalAsync<RestResponse<List<Attachment>>>($"api/now/attachment?sysparm_query=table_name={tableName}^table_sys_id={tableSysId}", cancellationToken).ConfigureAwait(false)).Item ?? [];

	/// <summary>
	/// Download a specified attachment to the local file system
	/// </summary>
	/// <param name="attachment">The attachment to download</param>
	/// <param name="outputPath">The path to store the attachment content in</param>
	/// <param name="filename">Optional filename for the file, defaults to filename from ServiceNow if unspecified</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>The path of the downloaded file</returns>
	public async Task<string> DownloadAttachmentAsync(
		Attachment attachment,
		string outputPath,
		[Optional] string? filename,
		[Optional] CancellationToken cancellationToken)
	{
		var actualFilename = filename ?? attachment.FileName ?? throw new ArgumentException("Filename must be provided or available from attachment", nameof(filename));
		var fileToWriteTo = Path.Combine(outputPath, actualFilename);
		using var response = await _httpClient.GetAsync(attachment.DownloadLink, cancellationToken).ConfigureAwait(false);
		using var streamToReadFrom = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
		using Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create);
		await streamToReadFrom.CopyToAsync(streamToWriteTo, cancellationToken).ConfigureAwait(false);
		response.Content = null;

		return fileToWriteTo;
	}

	/// <summary>
	/// Upload an attachment to a record using the Attachment API (api/now/attachment/file).
	/// The stream is sent as the raw request body, which is what that endpoint expects.
	/// </summary>
	/// <param name="tableName">The name of the table the record is in</param>
	/// <param name="tableSysId">The sys_id of the record to attach the file to</param>
	/// <param name="fileName">The file name to store the attachment under</param>
	/// <param name="content">The file content</param>
	/// <param name="contentType">The content type, defaults to application/octet-stream</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>The created attachment metadata</returns>
	public async Task<Attachment?> UploadAttachmentAsync(
		string tableName,
		string tableSysId,
		string fileName,
		Stream content,
		[Optional] string? contentType,
		[Optional] CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
		ArgumentException.ThrowIfNullOrWhiteSpace(tableSysId);
		ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
		ArgumentNullException.ThrowIfNull(content);

		var url = $"api/now/attachment/file?table_name={HttpUtility.UrlEncode(tableName)}&table_sys_id={HttpUtility.UrlEncode(tableSysId)}&file_name={HttpUtility.UrlEncode(fileName)}";
		using var request = new HttpRequestMessage(HttpMethod.Post, url)
		{
			Content = new StreamContent(content)
		};
		request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);

		using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
		await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
		return (await GetDeserializedObjectFromResponse<RestResponse<Attachment>>(response, Guid.NewGuid()).ConfigureAwait(false)).Item;
	}
}

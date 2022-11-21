using System.Runtime.Serialization;

namespace ServiceNow.Api;

[DataContract]
public class Attachment : CmdbEntity
{
	[DataMember(Name = "size_bytes")]
	public string? SizeBytes { get; set; }

	[DataMember(Name = "file_name")]
	public string? FileName { get; set; }

	[DataMember(Name = "average_image_color")]
	public string? AverageImageColor { get; set; }

	[DataMember(Name = "image_width")]
	public string? ImageWidth { get; set; }

	[DataMember(Name = "sys_updated_on")]
	public string? SysUpdatedOn { get; set; }

	[DataMember(Name = "sys_tags")]
	public string? SysTags { get; set; }

	[DataMember(Name = "table_name")]
	public string? TableName { get; set; }

	[DataMember(Name = "image_height")]
	public string? ImageHeight { get; set; }

	[DataMember(Name = "sys_updated_by")]
	public string? SysUpdatedBy { get; set; }

	[DataMember(Name = "download_link")]
	public string? DownloadLink { get; set; }

	[DataMember(Name = "content_type")]
	public string? ContentType { get; set; }

	[DataMember(Name = "sys_created_on")]
	public string? SysCreatedOn { get; set; }

	[DataMember(Name = "size_compressed")]
	public string? SizeCompressed { get; set; }

	[DataMember(Name = "compressed")]
	public string? Compressed { get; set; }

	[DataMember(Name = "table_sys_id")]
	public string? TableSysId { get; set; }

	[DataMember(Name = "chunk_size_bytes")]
	public string? ChunkSizeBytes { get; set; }

	[DataMember(Name = "sys_created_by")]
	public string? SysCreatedBy { get; set; }
}
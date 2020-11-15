using System.Runtime.Serialization;

namespace ServiceNow.Api.MetaData
{
	[DataContract]
	public class MetaAttribute
	{
		[DataMember(Name = "reference")]
		public string? Reference { get; set; }

		[DataMember(Name = "is_inherited")]
		public string? IsInherited { get; set; }

		[DataMember(Name = "is_mandatory")]
		public string? IsMandatory { get; set; }

		[DataMember(Name = "is_read_only")]
		public string? IsReadOnly { get; set; }

		[DataMember(Name = "default_value")]
		public object? DefaultValue { get; set; }

		[DataMember(Name = "label")]
		public string? Label { get; set; }

		[DataMember(Name = "type")]
		public string? Type { get; set; }

		[DataMember(Name = "element")]
		public string? Element { get; set; }

		[DataMember(Name = "max_length")]
		public string? MaxLength { get; set; }

		[DataMember(Name = "is_display")]
		public string? IsDisplay { get; set; }
	}
}
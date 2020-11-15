using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ServiceNow.Api.MetaData
{
	[DataContract]
	public class MetaDataResult
	{
		[DataMember(Name = "icon_url")]
		public string? IconUrl { get; set; }

		[DataMember(Name = "is_extendable")]
		public bool IsExtendable { get; set; }

		[DataMember(Name = "parent")]
		public string? Parent { get; set; }

		[DataMember(Name = "children")]
		public string[]? Children { get; set; }

		[DataMember(Name = "name")]
		public string? Name { get; set; }

		[DataMember(Name = "icon")]
		public string? Icon { get; set; }

		[DataMember(Name = "attributes")]
		public List<MetaAttribute>? Attributes { get; set; }

		[DataMember(Name = "label")]
		public string? Label { get; set; }
	}
}
using System.Runtime.Serialization;
using ServiceNow.Api.Attributes;

namespace ServiceNow.Api.Tables
{
	[DataContract]
	[TableName("core_country")]
	public class Country : CmdbCi
	{
		[DataMember(Name = "iso3166_3")]
		public string Iso31663 { get; set; }

		[DataMember(Name = "active")]
		public string Active { get; set; }

		[DataMember(Name = "iso3166_2")]
		public string Iso31662 { get; set; }

		[DataMember(Name = "un_numeric")]
		public string UnNumeric { get; set; }

		[DataMember(Name = "sys_tags")]
		public string SysTags { get; set; }

		[DataMember(Name = "population")]
		public string Population { get; set; }

		[DataMember(Name = "iana")]
		public string Iana { get; set; }

		[DataMember(Name = "order")]
		public string Order { get; set; }
	}
}
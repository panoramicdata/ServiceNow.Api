using ServiceNow.Api.Attributes;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables
{
	[DataContract]
	[TableName("cmn_department")]
	public class Department : CmdbCi
	{
		[DataMember(Name = "parent")]
		public string? Parent { get; set; }

		[DataMember(Name = "description")]
		public string? Description { get; set; }

		[DataMember(Name = "head_count")]
		public string? HeadCount { get; set; }

		[DataMember(Name = "sys_tags")]
		public string? SysTags { get; set; }

		[DataMember(Name = "dept_head")]
		public ResourceLink<User>? DeptHead { get; set; }

		[DataMember(Name = "id")]
		public string? Id { get; set; }

		[DataMember(Name = "primary_contact")]
		public string? PrimaryContact { get; set; }
	}
}
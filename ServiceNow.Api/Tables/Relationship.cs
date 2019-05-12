using System.Runtime.Serialization;
using ServiceNow.Api.Attributes;

namespace ServiceNow.Api.Tables
{
	[DataContract]
	[TableName("cmdb_rel_ci")]
	public class Relationship : Table
	{
		[DataMember(Name = "connection_strength")]
		public string ConnectionStrength { get; set; }

		[DataMember(Name = "parent")]
		public ResourceLink<CmdbCi> Parent { get; set; }

		[DataMember(Name = "sys_mod_count")]
		public string SysModCount { get; set; }

		[DataMember(Name = "sys_updated_on")]
		public string SysUpdatedOn { get; set; }

		[DataMember(Name = "sys_tags")]
		public string SysTags { get; set; }

		[DataMember(Name = "type")]
		public ResourceLink<CiRelationshipType> Type { get; set; }

		[DataMember(Name = "sys_updated_by")]
		public string SysUpdatedBy { get; set; }

		[DataMember(Name = "port")]
		public string Port { get; set; }

		[DataMember(Name = "sys_created_on")]
		public string SysCreatedOn { get; set; }

		[DataMember(Name = "percent_outage")]
		public string PercentOutage { get; set; }

		[DataMember(Name = "sys_created_by")]
		public string SysCreatedBy { get; set; }

		[DataMember(Name = "child")]
		public ResourceLink<CmdbCi> Child { get; set; }
	}
}
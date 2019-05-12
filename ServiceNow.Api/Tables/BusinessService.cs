using ServiceNow.Api.Attributes;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables
{
	[DataContract]
	[TableName("cmdb_ci_service")]
	public class BusinessService : CmdbCi
	{
		[DataMember(Name = "parent")]
		public ResourceLink<BusinessService> Parent { get; set; }

		[DataMember(Name = "used_for")]
		public string UsedFor { get; set; }

		// TODO - On "London" this is a ResourceLink to an SLA type which we don't have
		//[DataMember(Name="sla")]
		//public string Sla { get; set; }

		[DataMember(Name = "version")]
		public string Version { get; set; }

		[DataMember(Name = "busines_criticality")]
		public string BusinessCriticality { get; set; }

		[DataMember(Name = "user_group")]
		public string UserGroup { get; set; }

		[DataMember(Name = "mac_address")]
		public string MacAddress { get; set; }

		[DataMember(Name = "sys_tags")]
		public string SysTags { get; set; }

		[DataMember(Name = "service_classification")]
		public string ServiceClassification { get; set; }
	}
}
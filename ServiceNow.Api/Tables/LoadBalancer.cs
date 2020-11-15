using ServiceNow.Api.Attributes;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables
{
	[DataContract]
	[TableName("cmdb_ci_lb")]
	public class LoadBalancer : Server
	{
		[DataMember(Name = "dr_backup")]
		public string? DrBackup { get; set; }

		[DataMember(Name = "hardware_substatus")]
		public string? HardwareSubstatus { get; set; }

		[DataMember(Name = "mac_address")]
		public string? MacAddress { get; set; }

		[DataMember(Name = "sys_tags")]
		public string? SysTags { get; set; }

		[DataMember(Name = "host_name")]
		public string? HostName { get; set; }
	}
}
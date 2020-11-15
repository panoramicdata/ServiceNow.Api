using ServiceNow.Api.Attributes;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables
{
	[DataContract]
	[TableName("cmdb_ci_peripheral")]
	public class ComputerPeripheral : CmdbCi
	{
		[DataMember(Name = "type")]
		public string? Type { get; set; }

		[DataMember(Name = "computer")]
		public string? Computer { get; set; }

		[DataMember(Name = "managed_domain")]
		public string? ManagedDomain { get; set; }

		[DataMember(Name = "sys_class_path")]
		public string? SysClassPath { get; set; }

		[DataMember(Name = "mac_address")]
		public string? MacAddress { get; set; }

		[DataMember(Name = "sys_tags")]
		public string? SysTags { get; set; }
	}
}
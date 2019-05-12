using System.Runtime.Serialization;
using ServiceNow.Api.Attributes;

namespace ServiceNow.Api.Tables
{
	[DataContract]
	[TableName("cmdb_ci_hyper_v_instance")]
	public class HyperVVirtualMachineInstance : VirtualMachineInstance
	{
		[DataMember(Name = "baseboard_serial")]
		public string BaseboardSerial { get; set; }

		[DataMember(Name = "chassis_serial")]
		public string ChassisSerial { get; set; }

		[DataMember(Name = "object_id")]
		public string ObjectId { get; set; }

		[DataMember(Name = "bios_serial")]
		public string BiosSerial { get; set; }

		[DataMember(Name = "server")]
		public Server Server { get; set; }

		[DataMember(Name = "mac_address")]
		public string MacAddress { get; set; }

		[DataMember(Name = "bios_guid")]
		public string BiosGuid { get; set; }

		[DataMember(Name = "sys_tags")]
		public string SysTags { get; set; }
	}
}
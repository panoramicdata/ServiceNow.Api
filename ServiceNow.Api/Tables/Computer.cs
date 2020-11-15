using ServiceNow.Api.Attributes;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables
{
	[TableName("cmdb_ci_computer")]
	[DataContract]
	public class Computer : Hardware
	{
		[DataMember(Name = "ram")]
		public string? Ram { get; set; }

		[DataMember(Name = "disk_space")]
		public string? DiskSpace { get; set; }

		[DataMember(Name = "cpu_name")]
		public string? CpuName { get; set; }

		[DataMember(Name = "cpu_type")]
		public string? CpuType { get; set; }

		[DataMember(Name = "cpu_count")]
		public string? CpuCount { get; set; }

		[DataMember(Name = "cpu_core_count")]
		public string? CpuCoreCount { get; set; }

		[DataMember(Name = "cpu_core_thread")]
		public string? CpuCoreThread { get; set; }

		[DataMember(Name = "cpu_speed")]
		public string? CpuSpeed { get; set; }

		[DataMember(Name = "cpu_manufacturer")]
		public ResourceLink<Company>? CpuManufacturer { get; set; }

		[DataMember(Name = "floppy")]
		public string? Floppy { get; set; }

		[DataMember(Name = "form_factor")]
		public string? FormFactor { get; set; }

		[DataMember(Name = "chassis_type")]
		public string? ChassisType { get; set; }

		[DataMember(Name = "cd_rom")]
		public string? CdRom { get; set; }

		[DataMember(Name = "cd_speed")]
		public string? CdSpeed { get; set; }

		[DataMember(Name = "os")]
		public string? Os { get; set; }

		[DataMember(Name = "os_address_width")]
		public string? OsAddressWidth { get; set; }

		[DataMember(Name = "os_version")]
		public string? OsVersion { get; set; }

		[DataMember(Name = "os_service_pack")]
		public string? OsServicePack { get; set; }

		[DataMember(Name = "os_domain")]
		public string? OsDomain { get; set; }

		[DataMember(Name = "virtual")]
		public string? Virtual { get; set; }
	}
}
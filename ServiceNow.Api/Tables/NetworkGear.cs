using ServiceNow.Api.Attributes;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables
{
	[TableName("cmdb_ci_netgear")]
	[DataContract]
	public class NetworkGear : Hardware
	{
		[DataMember(Name = "device_type")]
		public string? DeviceType { get; set; }

		[DataMember(Name = "bandwidth")]
		public string? Bandwidth { get; set; }

		[DataMember(Name = "ports")]
		public string? Ports { get; set; }

		[DataMember(Name = "range")]
		public string? Range { get; set; }

		[DataMember(Name = "channels")]
		public string? Channels { get; set; }

		[DataMember(Name = "ram")]
		public string? Ram { get; set; }

		[DataMember(Name = "disk_space")]
		public string? DiskSpace { get; set; }

		[DataMember(Name = "cpu_type")]
		public string? CpuType { get; set; }

		[DataMember(Name = "cpu_count")]
		public string? CpuCount { get; set; }

		[DataMember(Name = "cpu_speed")]
		public string? CpuSpeed { get; set; }

		[DataMember(Name = "cpu_manufacturer")]
		public ResourceLink<Company>? CpuManufacturer { get; set; }

		[DataMember(Name = "firmware_manufacturer")]
		public ResourceLink<Company>? FirmwareManufacturer { get; set; }

		[DataMember(Name = "firmware_version")]
		public string? FirmwareVersion { get; set; }

		[DataMember(Name = "can_route")]
		public string? CanRoute { get; set; }

		[DataMember(Name = "can_switch")]
		public string? CanSwitch { get; set; }

		[DataMember(Name = "can_hub")]
		public string? CanHub { get; set; }

		[DataMember(Name = "can_partitionVlans")]
		public string? CanPartitionVlans { get; set; }

		[DataMember(Name = "physical_interface_count")]
		public string? PhysicalInterfaceCount { get; set; }

		[DataMember(Name = "discovery_proto_id")]
		public string? DiscoveryProtocolId { get; set; }

		[DataMember(Name = "discovery_proto_type")]
		public string? DiscoveryProtocolType { get; set; }
	}
}
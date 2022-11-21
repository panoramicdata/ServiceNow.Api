using ServiceNow.Api.Attributes;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables;

[TableName("cmdb_ci_hardware")]
[DataContract]
public class Hardware : CmdbCi
{
	[DataMember(Name = "hardware_status")]
	public string? HardwareStatus { get; set; }

	[DataMember(Name = "hardware_substatus")]
	public string? HardwareSubStatus { get; set; }

	[DataMember(Name = "default_gateway")]
	public string? DefaultGateway { get; set; }
}
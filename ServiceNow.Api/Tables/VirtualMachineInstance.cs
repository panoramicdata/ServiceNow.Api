using ServiceNow.Api.Attributes;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables;

[DataContract]
[TableName("cmdb_ci_vm_instance")]
public class VirtualMachineInstance : CmdbCi
{
	[DataMember(Name = "state")]
	public string? State { get; set; }

	[DataMember(Name = "cpus")]
	public string? Cpus { get; set; }

	[DataMember(Name = "memory")]
	public string? Memory { get; set; }

	[DataMember(Name = "disks")]
	public string? Disks { get; set; }

	[DataMember(Name = "disks_size")]
	public string? DisksSize { get; set; }

	[DataMember(Name = "nics")]
	public string? Nics { get; set; }
}
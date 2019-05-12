using System.Runtime.Serialization;
using ServiceNow.Api.Attributes;

namespace ServiceNow.Api.Tables
{
	[DataContract]
	[TableName("cmdb_ci_kvm_vm_instance")]
	public class KvmVirtualMachineInstance : VirtualMachineInstance
	{
		[DataMember(Name = "details_xml")]
		public string DetailsXml { get; set; }
	}
}
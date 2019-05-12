using System.Runtime.Serialization;
using ServiceNow.Api.Attributes;

namespace ServiceNow.Api.Tables
{
	[DataContract]
	[TableName("cmdb_ci_azure_vm")]
	public class AzureVirtualMachineInstance : VirtualMachineInstance
	{
	}
}
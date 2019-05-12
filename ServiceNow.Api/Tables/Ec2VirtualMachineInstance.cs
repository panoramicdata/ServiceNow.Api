using System.Runtime.Serialization;
using ServiceNow.Api.Attributes;

namespace ServiceNow.Api.Tables
{
	[DataContract]
	[TableName("cmdb_ci_ec2_instance")]
	public class Ec2VirtualMachineInstance : VirtualMachineInstance
	{
	}
}
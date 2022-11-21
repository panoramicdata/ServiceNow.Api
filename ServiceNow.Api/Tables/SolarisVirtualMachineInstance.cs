using System.Runtime.Serialization;
using ServiceNow.Api.Attributes;

namespace ServiceNow.Api.Tables;

[DataContract]
[TableName("cmdb_ci_solaris_instance")]
public class SolarisVirtualMachineInstance : VirtualMachineInstance
{
}
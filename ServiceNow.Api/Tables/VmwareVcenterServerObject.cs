using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables
{
	[DataContract]
	public abstract class VmwareVcenterServerObject : VirtualizationServer
	{
		[DataMember(Name="morid")]
		public string Morid { get; set; }
	}
}
using System.Runtime.Serialization;

namespace ServiceNow.Api.Example;

[DataContract]
public class WinServerModel
{
	[DataMember(Name = "sys_id")]
	public string Id { get; set; }

	[DataMember(Name = "name")]
	public string Name { get; set; }
}

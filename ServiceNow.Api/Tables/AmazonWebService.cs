using ServiceNow.Api.Attributes;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables;

[DataContract]
[TableName("amazon_web_service")]
public class AmazonWebService : PackagedTable
{
	[DataMember(Name = "api_version")]
	public string? ApiVersion { get; set; }

	[DataMember(Name = "sys_scope")]
	public ResourceLink<SysScope>? SysScope { get; set; }

	[DataMember(Name = "default_endpoint")]
	public string? DefaultEndpoint { get; set; }

	[DataMember(Name = "algorithm")]
	public string? Algorithm { get; set; }
}

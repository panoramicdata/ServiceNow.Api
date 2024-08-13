using Newtonsoft.Json;
using ServiceNow.Api.Converters;
using ServiceNow.Api.Interfaces;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables;

[DataContract]
[JsonConverter(typeof(ResourceLinkConverter))]
public class ResourceLink<T> : IResourceLink
{
	[DataMember(Name = "link")]
	public string? Link { get; set; } // REST URL for child record

	[DataMember(Name = "value")]
	public string? Value { get; set; } // Reference to the child record (sys_id)

	// The ToString Method is called on serialization back to servicenow through the ResourceLinkConverter
	public override string ToString() => Value ?? string.Empty;
}
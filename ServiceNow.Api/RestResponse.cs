using System.Runtime.Serialization;

namespace ServiceNow.Api;

[DataContract]
public class RestResponse<T>
{
	[DataMember(Name = "result")]
	public T? Item { get; set; }
}
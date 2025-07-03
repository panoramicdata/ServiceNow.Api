using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ServiceNow.Api;

[DataContract]
public class Page<T> : RestListResponseBase
{
	[DataMember(Name = "result")]
	public List<T> Items { get; set; } = [];
}
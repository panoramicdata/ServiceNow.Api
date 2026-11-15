using ServiceNow.Api.Attributes;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables;

[DataContract]
[TableName("cmdb_rel_type")]
public class CiRelationshipType : PackagedTable
{
	[DataMember(Name = "child_descriptor")]
	public string? ChildDescriptor { get; set; }

	[DataMember(Name = "end_point")]
	public string? EndPoint { get; set; }

	[DataMember(Name = "parent_descriptor")]
	public string? ParentDescriptor { get; set; }
}

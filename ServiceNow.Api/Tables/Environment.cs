using ServiceNow.Api.Attributes;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables
{
	[DataContract]
	[TableName("cmdb_ci_environment")]
	public class Environment : CmdbCi
	{
		[DataMember(Name = "version")]
		public string? Version { get; set; }
	}
}
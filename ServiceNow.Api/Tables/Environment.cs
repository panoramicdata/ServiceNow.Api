using System.Runtime.Serialization;
using ServiceNow.Api.Attributes;

namespace ServiceNow.Api.Tables
{
	[DataContract]
	[TableName("cmdb_ci_environment")]
	public class Environment : CmdbCi
	{
		[DataMember(Name = "version")]
		public string Version { get; set; }
	}
}
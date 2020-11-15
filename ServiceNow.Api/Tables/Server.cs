using ServiceNow.Api.Attributes;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables
{
	[DataContract]
	[TableName("cmdb_ci_server")]
	public class Server : Computer
	{
		[DataMember(Name = "host_name")]
		public string? Hostname { get; set; }

		[DataMember(Name = "classification")]
		public string? Classification { get; set; }

		[DataMember(Name = "used_for")]
		public string? UsedFor { get; set; }

		[DataMember(Name = "firewall_status")]
		public string? FirewallStatus { get; set; }
	}
}
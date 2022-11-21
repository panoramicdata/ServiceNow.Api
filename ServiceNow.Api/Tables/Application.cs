using ServiceNow.Api.Attributes;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables;

[DataContract]
[TableName("cmdb_ci_appl")]
public class Application : CmdbCi
{
	[DataMember(Name = "tcp_port")]
	public string? TcpPort { get; set; }

	[DataMember(Name = "running_process_command")]
	public string? RunningProcessCommand { get; set; }

	[DataMember(Name = "pid")]
	public string? Pid { get; set; }

	[DataMember(Name = "running_process_parameters")]
	public string? RunningProcessParameters { get; set; }

	[DataMember(Name = "running_process_key_parameters")]
	public string? RunningProcessKeyParameters { get; set; }

	[DataMember(Name = "rp_command_hash")]
	public string? RpCommandHash { get; set; }

	[DataMember(Name = "install_directory")]
	public string? InstallDirectory { get; set; }

	[DataMember(Name = "used_for")]
	public string? UsedFor { get; set; }

	[DataMember(Name = "is_clustered")]
	public string? IsClustered { get; set; }

	[DataMember(Name = "rp_key_parameters_hash")]
	public string? RpKeyParametersHash { get; set; }

	[DataMember(Name = "version")]
	public string? Version { get; set; }

	[DataMember(Name = "edition")]
	public string? Edition { get; set; }

	[DataMember(Name = "mac_address")]
	public string? MacAddress { get; set; }

	[DataMember(Name = "config_file")]
	public string? ConfigFile { get; set; }

	[DataMember(Name = "sys_tags")]
	public string? SysTags { get; set; }

	[DataMember(Name = "config_directory")]
	public string? ConfigDirectory { get; set; }
}

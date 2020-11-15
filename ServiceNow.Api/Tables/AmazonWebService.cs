using ServiceNow.Api.Attributes;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables
{
	[DataContract]
	[TableName("amazon_web_service")]
	public class AmazonWebService : Table
	{
		[DataMember(Name = "sys_replace_on_upgrade")]
		public string? SysReplaceOnUpgrade { get; set; }

		[DataMember(Name = "sys_mod_count")]
		public string? SysModCount { get; set; }

		[DataMember(Name = "sys_updated_on")]
		public string? SysUpdatedOn { get; set; }

		[DataMember(Name = "api_version")]
		public string? ApiVersion { get; set; }

		[DataMember(Name = "sys_tags")]
		public string? SysTags { get; set; }

		[DataMember(Name = "sys_class_name")]
		public string? SysClassName { get; set; }

		[DataMember(Name = "sys_package")]
		public ResourceLink<SysPackage>? SysPackage { get; set; }

		[DataMember(Name = "sys_update_name")]
		public string? SysUpdateName { get; set; }

		[DataMember(Name = "sys_updated_by")]
		public string? SysUpdatedBy { get; set; }

		[DataMember(Name = "sys_created_on")]
		public string? SysCreatedOn { get; set; }

		[DataMember(Name = "name")]
		public string? Name { get; set; }

		[DataMember(Name = "sys_name")]
		public string? SysName { get; set; }

		[DataMember(Name = "sys_scope")]
		public ResourceLink<SysScope>? SysScope { get; set; }

		[DataMember(Name = "sys_customer_update")]
		public string? SysCustomerUpdate { get; set; }

		[DataMember(Name = "default_endpoint")]
		public string? DefaultEndpoint { get; set; }

		[DataMember(Name = "sys_created_by")]
		public string? SysCreatedBy { get; set; }

		[DataMember(Name = "algorithm")]
		public string? Algorithm { get; set; }

		[DataMember(Name = "sys_policy")]
		public string? SysPolicy { get; set; }
	}
}
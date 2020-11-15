using ServiceNow.Api.Attributes;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables
{
	[TableName("cmdb_ci")]
	[DataContract]
	public class CmdbCi : Table
	{
		[DataMember(Name = "sys_class_name")]
		public string? SysClassName { get; set; }

		[DataMember(Name = "name")]
		public string? Name { get; set; }

		[DataMember(Name = "asset_tag")]
		public string? AssetTag { get; set; }

		[DataMember(Name = "serial_number")]
		public string? SerialNumber { get; set; }

		[DataMember(Name = "category")]
		public string? Category { get; set; }

		[DataMember(Name = "subcategory")]
		public string? Subcategory { get; set; }

		[DataMember(Name = "assigned_to")]
		public ResourceLink<User>? AssignedTo { get; set; }

		[DataMember(Name = "company")]
		public ResourceLink<Company>? Company { get; set; }

		[DataMember(Name = "assigned")]
		public string? Assigned { get; set; }

		[DataMember(Name = "assignment_group")]
		public ResourceLink<AssignmentGroup>? AssignmentGroup { get; set; }

		[DataMember(Name = "install_status")]
		public string? InstallStatus { get; set; }

		[DataMember(Name = "operational_status")]
		public string? OperationalStatus { get; set; }

		[DataMember(Name = "purchase_date")]
		public string? PurchaseDate { get; set; }

		[DataMember(Name = "fault_count")]
		public string? FaultCount { get; set; }

		[DataMember(Name = "order_date")]
		public string? OrderDate { get; set; }

		[DataMember(Name = "delivery_date")]
		public string? DeliveryDate { get; set; }

		[DataMember(Name = "install_date")]
		public string? InstallDate { get; set; }

		[DataMember(Name = "manufacturer")]
		public ResourceLink<Company>? Manufacturer { get; set; }

		[DataMember(Name = "vendor")]
		public ResourceLink<Company>? Vendor { get; set; }

		[DataMember(Name = "model_number")]
		public string? ModelNumber { get; set; }

		[DataMember(Name = "model_id")]
		public ResourceLink<Model>? ModelId { get; set; }

		[DataMember(Name = "justification")]
		public string? Justification { get; set; }

		[DataMember(Name = "short_description")]
		public string? ShortDescription { get; set; }

		[DataMember(Name = "comments")]
		public string? Comments { get; set; }

		[DataMember(Name = "location")]
		public ResourceLink<Location>? Location { get; set; }

		[DataMember(Name = "department")]
		public ResourceLink<Department>? Department { get; set; }

		[DataMember(Name = "lease_id")]
		public string? LeaseId { get; set; }

		[DataMember(Name = "warranty_expiration")]
		public string? WarrantyExpiration { get; set; }

		[DataMember(Name = "po_number")]
		public string? PoNumber { get; set; }

		[DataMember(Name = "invoice_number")]
		public string? InvoiceNumber { get; set; }

		[DataMember(Name = "gl_account")]
		public string? GlAccount { get; set; }

		[DataMember(Name = "cost")]
		public string? Cost { get; set; }

		[DataMember(Name = "cost_cc")]
		public string? CostCc { get; set; }

		[DataMember(Name = "discovery_source")]
		public string? DiscoverySource { get; set; }

		[DataMember(Name = "first_discovered")]
		public string? FirstDiscovered { get; set; }

		[DataMember(Name = "last_discovered")]
		public string? LastDiscovered { get; set; }

		[DataMember(Name = "start_date")]
		public string? StartDate { get; set; }

		[DataMember(Name = "change_control")]
		public ResourceLink<UserGroup>? ChangeControl { get; set; }

		[DataMember(Name = "checked_in")]
		public string? CheckedIn { get; set; }

		[DataMember(Name = "checked_out")]
		public string? CheckedOut { get; set; }

		[DataMember(Name = "due")]
		public string? Due { get; set; }

		[DataMember(Name = "due_in")]
		public string? DueIn { get; set; }

		[DataMember(Name = "support_group")]
		public ResourceLink<UserGroup>? SupportGroup { get; set; }

		[DataMember(Name = "supported_by")]
		public ResourceLink<User>? SupportedBy { get; set; }

		[DataMember(Name = "owned_by")]
		public ResourceLink<User>? OwnedBy { get; set; }

		[DataMember(Name = "managed_by")]
		public ResourceLink<User>? ManagedBy { get; set; }

		[DataMember(Name = "monitor")]
		public string? Monitor { get; set; }

		[DataMember(Name = "attributes")]
		public string? Attributes { get; set; }

		[DataMember(Name = "ip_address")]
		public string? IpAddress { get; set; }

		[DataMember(Name = "can_print")]
		public string? CanPrint { get; set; }

		[DataMember(Name = "cost_center")]
		public ResourceLink<CostCenter>? CostCenter { get; set; }

		[DataMember(Name = "sys_domain")]
		public ResourceLink<SysDomain>? SysDomain { get; set; }

		[DataMember(Name = "correlation_id")]
		public string? CorrelationId { get; set; }

		[DataMember(Name = "dns_domain")]
		public string? DnsDomain { get; set; }

		[DataMember(Name = "asset")]
		public ResourceLink<Asset>? Asset { get; set; }

		[DataMember(Name = "skip_sync")]
		public string? SkipSync { get; set; }

		[DataMember(Name = "unverified")]
		public string? Unverified { get; set; }

		[DataMember(Name = "fqdn")]
		public string? Fqdn { get; set; }

		[DataMember(Name = "sys_updated_by")]
		public string? SysUpdatedBy { get; set; }

		[DataMember(Name = "sys_updated_on")]
		public string? SysUpdatedOn { get; set; }

		[DataMember(Name = "sys_created_by")]
		public string? SysCreatedBy { get; set; }

		[DataMember(Name = "sys_created_on")]
		public string? SysCreatedOn { get; set; }

		[DataMember(Name = "sys_mod_count")]
		public string? SysModCount { get; set; }

		[DataMember(Name = "maintenance_schedule")]
		public ResourceLink<Schedule>? MaintenanceSchedule { get; set; }

		[DataMember(Name = "schedule")]
		public ResourceLink<Schedule>? Schedule { get; set; }

		[DataMember(Name = "sys_domain_path")]
		public string? SysDomainPath { get; set; }
	}
}
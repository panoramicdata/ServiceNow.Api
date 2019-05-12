using ServiceNow.Api.Attributes;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables
{
	[DataContract]
	[TableName("sla")]
	public class Agreement : Table
	{
		[DataMember(Name = "responsible_user")]
		public string ResponsibleUser { get; set; }

		[DataMember(Name = "short_description")]
		public string ShortDescription { get; set; }

		[DataMember(Name = "incident_procedures")]
		public string IncidentProcedures { get; set; }

		[DataMember(Name = "notes")]
		public string Notes { get; set; }

		[DataMember(Name = "security_notes")]
		public string SecurityNotes { get; set; }

		[DataMember(Name = "technical_lead")]
		public string TechnicalLead { get; set; }

		[DataMember(Name = "change_procedures")]
		public string ChangeProcedures { get; set; }

		[DataMember(Name = "description")]
		public string Description { get; set; }

		[DataMember(Name = "consultant_user")]
		public string ConsultantUser { get; set; }

		[DataMember(Name = "sys_updated_on")]
		public string SysUpdatedOn { get; set; }

		[DataMember(Name = "signatures")]
		public string Signatures { get; set; }

		[DataMember(Name = "sys_class_name")]
		public string SysClassName { get; set; }

		[DataMember(Name = "begins")]
		public string Begins { get; set; }

		[DataMember(Name = "avail_pct")]
		public string AvailPct { get; set; }

		[DataMember(Name = "number")]
		public string Number { get; set; }

		[DataMember(Name = "sys_updated_by")]
		public string SysUpdatedBy { get; set; }

		[DataMember(Name = "sys_created_on")]
		public string SysCreatedOn { get; set; }

		[DataMember(Name = "ends")]
		public string Ends { get; set; }

		[DataMember(Name = "department")]
		public ResourceLink<Department> Department { get; set; }

		[DataMember(Name = "sys_created_by")]
		public string SysCreatedBy { get; set; }

		[DataMember(Name = "calendar")]
		public ResourceLink<SysCalendar> Calendar { get; set; }

		[DataMember(Name = "disaster_recovery")]
		public string DisasterRecovery { get; set; }

		[DataMember(Name = "contract")]
		public string Contract { get; set; }

		[DataMember(Name = "sys_mod_count")]
		public string SysModCount { get; set; }

		[DataMember(Name = "active")]
		public string Active { get; set; }

		[DataMember(Name = "service_goals")]
		public string ServiceGoals { get; set; }

		[DataMember(Name = "sys_tags")]
		public string SysTags { get; set; }

		[DataMember(Name = "business_unit")]
		public string BusinessUnit { get; set; }

		[DataMember(Name = "informed_user")]
		public string InformedUser { get; set; }

		[DataMember(Name = "functional_area")]
		public string FunctionalArea { get; set; }

		[DataMember(Name = "users")]
		public string Users { get; set; }

		[DataMember(Name = "business_lead")]
		public string BusinessLead { get; set; }

		[DataMember(Name = "transaction_load")]
		public string TransactionLoad { get; set; }

		[DataMember(Name = "reponsibilities")]
		public string Reponsibilities { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "response_time")]
		public string ResponseTime { get; set; }

		[DataMember(Name = "maintenance")]
		public string Maintenance { get; set; }

		[DataMember(Name = "next_review")]
		public string NextReview { get; set; }

		[DataMember(Name = "accountable_user")]
		public string AccountableUser { get; set; }
	}
}

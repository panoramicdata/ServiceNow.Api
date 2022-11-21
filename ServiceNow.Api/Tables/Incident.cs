
using ServiceNow.Api.Attributes;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables;

[DataContract]
[TableName("incident")]
public class Incident : Table
{
	[DataMember(Name = "active")]
	public string? Active { get; set; }

	[DataMember(Name = "activity_due")]
	public string? ActivityDue { get; set; }

	[DataMember(Name = "additional_assignee_list")]
	public string? AdditionalAssigneeList { get; set; }

	[DataMember(Name = "approval")]
	public string? Approval { get; set; }

	[DataMember(Name = "approval_history")]
	public string? ApprovalHistory { get; set; }

	[DataMember(Name = "approval_set")]
	public string? ApprovalSet { get; set; }

	[DataMember(Name = "assigned_to")]
	public object? AssignedTo { get; set; }

	[DataMember(Name = "assignment_group")]
	public object? AssignmentGroup { get; set; }

	[DataMember(Name = "business_duration")]
	public string? BusinessDuration { get; set; }

	[DataMember(Name = "business_service")]
	public ResourceLink<Service>? BusinessService { get; set; }

	[DataMember(Name = "business_stc")]
	public string? BusinessStc { get; set; }

	[DataMember(Name = "calendar_duration")]
	public string? CalendarDuration { get; set; }

	[DataMember(Name = "calendar_stc")]
	public string? CalendarStc { get; set; }

	[DataMember(Name = "caller_id")]
	public ResourceLink<User>? CallerId { get; set; }

	[DataMember(Name = "category")]
	public string? Category { get; set; }

	[DataMember(Name = "caused_by")]
	public string? CausedBy { get; set; }

	[DataMember(Name = "child_incidents")]
	public string? ChildIncidents { get; set; }

	[DataMember(Name = "close_code")]
	public string? CloseCode { get; set; }

	[DataMember(Name = "closed_at")]
	public string? ClosedAt { get; set; }

	[DataMember(Name = "closed_by")]
	public object? ClosedBy { get; set; }

	[DataMember(Name = "close_notes")]
	public string? CloseNotes { get; set; }

	[DataMember(Name = "cmdb_ci")]
	public object? CmdbCi { get; set; }

	[DataMember(Name = "comments")]
	public string? Comments { get; set; }

	[DataMember(Name = "comments_and_work_notes")]
	public string? CommentsAndWorkNotes { get; set; }

	[DataMember(Name = "company")]
	public object? Company { get; set; }

	[DataMember(Name = "contact_type")]
	public string? ContactType { get; set; }

	[DataMember(Name = "correlation_display")]
	public string? CorrelationDisplay { get; set; }

	[DataMember(Name = "correlation_id")]
	public string? CorrelationId { get; set; }

	[DataMember(Name = "delivery_plan")]
	public string? DeliveryPlan { get; set; }

	[DataMember(Name = "delivery_task")]
	public string? DeliveryTask { get; set; }

	[DataMember(Name = "description")]
	public string? Description { get; set; }

	[DataMember(Name = "due_date")]
	public string? DueDate { get; set; }

	[DataMember(Name = "escalation")]
	public string? Escalation { get; set; }

	[DataMember(Name = "expected_start")]
	public string? ExpectedStart { get; set; }

	[DataMember(Name = "follow_up")]
	public string? FollowUp { get; set; }

	[DataMember(Name = "group_list")]
	public string? GroupList { get; set; }

	[DataMember(Name = "hold_reason")]
	public string? HoldReason { get; set; }

	[DataMember(Name = "impact")]
	public string? Impact { get; set; }

	[DataMember(Name = "incident_state")]
	public string? IncidentState { get; set; }

	[DataMember(Name = "knowledge")]
	public string? Knowledge { get; set; }

	[DataMember(Name = "location")]
	public object? Location { get; set; }

	[DataMember(Name = "made_sla")]
	public string? MadeSla { get; set; }

	[DataMember(Name = "notify")]
	public string? Notify { get; set; }

	[DataMember(Name = "number")]
	public string? Number { get; set; }

	[DataMember(Name = "opened_at")]
	public string? OpenedAt { get; set; }

	[DataMember(Name = "opened_by")]
	public ResourceLink<User>? OpenedBy { get; set; }

	[DataMember(Name = "order")]
	public string? Order { get; set; }

	[DataMember(Name = "parent")]
	public string? Parent { get; set; }
	[DataMember(Name = "parent_incident")]
	public ResourceLink<Incident>? ParentIncident { get; set; }

	[DataMember(Name = "priority")]
	public string? Priority { get; set; }

	[DataMember(Name = "problem_id")]
	public object? ProblemId { get; set; }

	[DataMember(Name = "reassignment_count")]
	public string? ReassignmentCount { get; set; }

	[DataMember(Name = "reopen_count")]
	public string? ReopenCount { get; set; }

	[DataMember(Name = "resolved_at")]
	public string? ResolvedAt { get; set; }

	[DataMember(Name = "resolved_by")]
	public object? ResolvedBy { get; set; }

	[DataMember(Name = "rfc")]
	public string? Rfc { get; set; }

	[DataMember(Name = "severity")]
	public string? Severity { get; set; }

	[DataMember(Name = "short_description")]
	public string? ShortDescription { get; set; }

	[DataMember(Name = "sla_due")]
	public string? SlaDue { get; set; }

	[DataMember(Name = "state")]
	public string? State { get; set; }

	[DataMember(Name = "subcategory")]
	public string? Subcategory { get; set; }

	[DataMember(Name = "sys_class_name")]
	public string? SysClassName { get; set; }

	[DataMember(Name = "sys_created_by")]
	public string? SysCreatedBy { get; set; }

	[DataMember(Name = "sys_created_on")]
	public string? SysCreatedOn { get; set; }

	[DataMember(Name = "sys_domain")]
	public ResourceLink<SysDomain>? SysDomain { get; set; }

	[DataMember(Name = "sys_domain_path")]
	public string? SysDomainPath { get; set; }

	[DataMember(Name = "sys_mod_count")]
	public string? SysModCount { get; set; }

	[DataMember(Name = "sys_tags")]
	public string? SysTags { get; set; }

	[DataMember(Name = "sys_updated_by")]
	public string? SysUpdatedBy { get; set; }

	[DataMember(Name = "sys_updated_on")]
	public string? SysUpdatedOn { get; set; }

	[DataMember(Name = "time_worked")]
	public string? TimeWorked { get; set; }

	[DataMember(Name = "upon_approval")]
	public string? UponApproval { get; set; }

	[DataMember(Name = "upon_reject")]
	public string? UponReject { get; set; }

	[DataMember(Name = "urgency")]
	public string? Urgency { get; set; }

	[DataMember(Name = "user_input")]
	public string? UserInput { get; set; }

	[DataMember(Name = "watch_list")]
	public string? WatchList { get; set; }
	[DataMember(Name = "work_end")]
	public string? WorkEnd { get; set; }

	[DataMember(Name = "work_notes")]
	public string? WorkNotes { get; set; }

	[DataMember(Name = "work_notes_list")]
	public string? WorkNotesList { get; set; }
	[DataMember(Name = "work_start")]
	public string? WorkStart { get; set; }
}
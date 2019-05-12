using System.Runtime.Serialization;
using ServiceNow.Api.Attributes;

namespace ServiceNow.Api.Tables
{
	[DataContract]
	[TableName("sys_user")]
	public class User : Table
	{
		[DataMember(Name = "calendar_integration")]
		public string CalendarIntegration { get; set; }

		[DataMember(Name = "country")]
		public string Country { get; set; }

		[DataMember(Name = "last_login_time")]
		public string LastLoginTime { get; set; }

		[DataMember(Name = "source")]
		public string Source { get; set; }

		[DataMember(Name = "sys_updated_on")]
		public string SysUpdatedOn { get; set; }

		[DataMember(Name = "building")]
		public string Building { get; set; }

		[DataMember(Name = "web_service_access_only")]
		public string WebServiceAccessOnly { get; set; }

		[DataMember(Name = "notification")]
		public string Notification { get; set; }

		[DataMember(Name = "sys_updated_by")]
		public string SysUpdatedBy { get; set; }

		[DataMember(Name = "sso_source")]
		public string SsoSource { get; set; }

		[DataMember(Name = "sys_created_on")]
		public string SysCreatedOn { get; set; }

		[DataMember(Name = "sys_domain")]
		public ResourceLink<SysDomain> SysDomain { get; set; }

		[DataMember(Name = "state")]
		public string State { get; set; }

		[DataMember(Name = "vip")]
		public string Vip { get; set; }

		[DataMember(Name = "sys_created_by")]
		public string SysCreatedBy { get; set; }

		[DataMember(Name = "zip")]
		public string Zip { get; set; }

		[DataMember(Name = "home_phone")]
		public string HomePhone { get; set; }

		[DataMember(Name = "time_format")]
		public string TimeFormat { get; set; }

		[DataMember(Name = "last_login")]
		public string LastLogin { get; set; }

		[DataMember(Name = "default_perspective")]
		public string DefaultPerspective { get; set; }

		[DataMember(Name = "active")]
		public string Active { get; set; }

		[DataMember(Name = "sys_domain_path")]
		public string SysDomainPath { get; set; }

		[DataMember(Name = "cost_center")]
		public ResourceLink<CostCenter> CostCenter { get; set; }

		[DataMember(Name = "phone")]
		public string Phone { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "employee_number")]
		public string EmployeeNumber { get; set; }

		[DataMember(Name = "password_needs_reset")]
		public string PasswordNeedsReset { get; set; }

		[DataMember(Name = "gender")]
		public string Gender { get; set; }

		[DataMember(Name = "city")]
		public string City { get; set; }

		[DataMember(Name = "failed_attempts")]
		public string FailedAttempts { get; set; }

		[DataMember(Name = "user_name")]
		public string UserName { get; set; }

		[DataMember(Name = "roles")]
		public string Roles { get; set; }

		[DataMember(Name = "title")]
		public string Title { get; set; }

		[DataMember(Name = "sys_class_name")]
		public string SysClassName { get; set; }

		[DataMember(Name = "internal_integration_user")]
		public string InternalIntegrationUser { get; set; }

		[DataMember(Name = "ldap_server")]
		public ResourceLink<LdapServerConfig> LdapServer { get; set; }

		[DataMember(Name = "mobile_phone")]
		public string MobilePhone { get; set; }

		[DataMember(Name = "street")]
		public string Street { get; set; }

		[DataMember(Name = "company")]
		public ResourceLink<Company> Company { get; set; }

		[DataMember(Name = "department")]
		public ResourceLink<Department> Department { get; set; }

		[DataMember(Name = "first_name")]
		public string FirstName { get; set; }

		[DataMember(Name = "email")]
		public string Email { get; set; }

		[DataMember(Name = "introduction")]
		public string Introduction { get; set; }

		[DataMember(Name = "preferred_language")]
		public string PreferredLanguage { get; set; }

		[DataMember(Name = "manager")]
		public ResourceLink<User> Manager { get; set; }

		[DataMember(Name = "locked_out")]
		public string LockedOut { get; set; }

		[DataMember(Name = "sys_mod_count")]
		public string SysModCount { get; set; }

		[DataMember(Name = "last_name")]
		public string LastName { get; set; }

		[DataMember(Name = "photo")]
		public string Photo { get; set; }

		[DataMember(Name = "middle_name")]
		public string MiddleName { get; set; }

		[DataMember(Name = "sys_tags")]
		public string SysTags { get; set; }

		[DataMember(Name = "time_zone")]
		public string TimeZone { get; set; }

		[DataMember(Name = "schedule")]
		public string Schedule { get; set; }

		[DataMember(Name = "date_format")]
		public string DateFormat { get; set; }

		[DataMember(Name = "location")]
		public ResourceLink<Location> Location { get; set; }
	}
}
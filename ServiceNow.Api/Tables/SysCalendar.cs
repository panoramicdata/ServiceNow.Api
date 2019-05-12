using ServiceNow.Api.Attributes;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables
{
	[DataContract]
	[TableName("sys_calendar")]
	public class SysCalendar : Table
	{
		// TODO Populate when required
	}
}

/*
{
	"result": {
		"sys_replace_on_upgrade": "false",
		"sys_mod_count": "2",
		"sys_updated_on": "2006-06-23 20:38:18",
		"sys_tags": "",
		"sys_class_name": "sys_calendar",
		"sys_id": "bea4c03cc611227801d411166792a145",
		"sys_package": {
			"link": "https://dev30623.service-now.com/api/now/table/sys_package/ea1abe336d21130092c063eaa9c7ca24",
			"value": "ea1abe336d21130092c063eaa9c7ca24"
		},
		"sys_update_name": "sys_calendar_bea4c03cc611227801d411166792a145",
		"sys_updated_by": "glide.maint",
		"sys_created_on": "2004-05-26 00:10:26",
		"name": "Monday thru Friday (9 - 5)",
		"sys_name": "Monday thru Friday (9 - 5)",
		"sys_scope": {
			"link": "https://dev30623.service-now.com/api/now/table/sys_scope/global",
			"value": "global"
		},
		"sys_customer_update": "false",
		"sys_created_by": "pat",
		"sys_policy": ""
	}
}
*/

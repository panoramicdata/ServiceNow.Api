using System.Runtime.Serialization;
using ServiceNow.Api.Attributes;

namespace ServiceNow.Api.Tables
{
	[DataContract]
	[TableName("cmn_cost_center")]
	public class CostCenter : CmdbCi
	{
		[DataMember(Name = "account_number")]
		public string AccountNumber { get; set; }

		[DataMember(Name = "parent")]
		public string Parent { get; set; }

		[DataMember(Name = "code")]
		public string Code { get; set; }

		[DataMember(Name = "manager")]
		public ResourceLink<User> Manager { get; set; }

		[DataMember(Name = "valid_from")]
		public string ValidFrom { get; set; }

		[DataMember(Name = "sys_tags")]
		public string SysTags { get; set; }

		[DataMember(Name = "valid_to")]
		public string ValidTo { get; set; }
	}
}
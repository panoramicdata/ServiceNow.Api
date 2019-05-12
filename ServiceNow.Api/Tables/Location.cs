using ServiceNow.Api.Attributes;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables
{
	[TableName("cmn_location")]
	[DataContract]
	public class Location : Table
	{
		[DataMember(Name = "country")]
		public string Country { get; set; }

		[DataMember(Name = "parent")]
		public ResourceLink<Location> Parent { get; set; }

		[DataMember(Name = "city")]
		public string City { get; set; }

		[DataMember(Name = "latitude")]
		public string Latitude { get; set; }

		[DataMember(Name = "sys_updated_on")]
		public string SysUpdatedOn { get; set; }

		[DataMember(Name = "sys_updated_by")]
		public string SysUpdatedBy { get; set; }

		[DataMember(Name = "stock_room")]
		public string StockRoom { get; set; }

		[DataMember(Name = "street")]
		public string Street { get; set; }

		[DataMember(Name = "sys_created_on")]
		public string SysCreatedOn { get; set; }

		[DataMember(Name = "contact")]
		public string Contact { get; set; }

		[DataMember(Name = "phone_territory")]
		public ResourceLink<SysPhoneTerritory> PhoneTerritory { get; set; }

		[DataMember(Name = "company")]
		public ResourceLink<Company> Company { get; set; }

		[DataMember(Name = "lat_long_error")]
		public string LatLongError { get; set; }

		[DataMember(Name = "state")]
		public string State { get; set; }

		[DataMember(Name = "sys_created_by")]
		public string SysCreatedBy { get; set; }

		[DataMember(Name = "longitude")]
		public string Longitude { get; set; }

		[DataMember(Name = "zip")]
		public string Zip { get; set; }

		[DataMember(Name = "sys_mod_count")]
		public string SysModCount { get; set; }

		[DataMember(Name = "sys_tags")]
		public string SysTags { get; set; }

		[DataMember(Name = "time_zone")]
		public string TimeZone { get; set; }

		[DataMember(Name = "full_name")]
		public string FullName { get; set; }

		[DataMember(Name = "fax_phone")]
		public string FaxPhone { get; set; }

		[DataMember(Name = "phone")]
		public string Phone { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }
	}
}
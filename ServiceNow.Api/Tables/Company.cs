using ServiceNow.Api.Attributes;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables
{
	[DataContract]
	[TableName("core_company")]
	public class Company : Table
	{
		[DataMember(Name = "banner_image_light")]
		public string? BannerImageLight { get; set; }

		[DataMember(Name = "country")]
		public string? Country { get; set; }

		[DataMember(Name = "parent")]
		public string? Parent { get; set; }

		[DataMember(Name = "notes")]
		public string? Notes { get; set; }

		[DataMember(Name = "city")]
		public string? City { get; set; }

		[DataMember(Name = "stock_symbol")]
		public string? StockSymbol { get; set; }

		[DataMember(Name = "latitude")]
		public string? Latitude { get; set; }

		[DataMember(Name = "discount")]
		public string? Discount { get; set; }

		[DataMember(Name = "sys_updated_on")]
		public string? SysUpdatedOn { get; set; }

		[DataMember(Name = "sys_class_name")]
		public string? SysClassName { get; set; }

		[DataMember(Name = "manufacturer")]
		public string? Manufacturer { get; set; }

		[DataMember(Name = "apple_icon")]
		public string? AppleIcon { get; set; }

		[DataMember(Name = "market_cap")]
		public string? MarketCap { get; set; }

		[DataMember(Name = "sys_updated_by")]
		public string? SysUpdatedBy { get; set; }

		[DataMember(Name = "num_employees")]
		public string? NumEmployees { get; set; }

		[DataMember(Name = "fiscal_year")]
		public string? FiscalYear { get; set; }

		[DataMember(Name = "rank_tier")]
		public string? RankTier { get; set; }

		[DataMember(Name = "sso_source")]
		public string? SsoSource { get; set; }

		[DataMember(Name = "street")]
		public string? Street { get; set; }

		[DataMember(Name = "sys_created_on")]
		public string? SysCreatedOn { get; set; }

		[DataMember(Name = "vendor")]
		public string? Vendor { get; set; }

		[DataMember(Name = "contact")]
		public string? Contact { get; set; }

		[DataMember(Name = "lat_long_error")]
		public string? LatLongError { get; set; }

		[DataMember(Name = "stock_price")]
		public string? StockPrice { get; set; }

		[DataMember(Name = "theme")]
		public string? Theme { get; set; }

		[DataMember(Name = "banner_image")]
		public string? BannerImage { get; set; }

		[DataMember(Name = "state")]
		public string? State { get; set; }

		[DataMember(Name = "sys_created_by")]
		public string? SysCreatedBy { get; set; }

		[DataMember(Name = "longitude")]
		public string? Longitude { get; set; }

		[DataMember(Name = "vendor_type")]
		public string? VendorType { get; set; }

		[DataMember(Name = "zip")]
		public string? Zip { get; set; }

		[DataMember(Name = "profits")]
		public string? Profits { get; set; }

		[DataMember(Name = "revenue_per_year")]
		public string? RevenuePerYear { get; set; }

		[DataMember(Name = "website")]
		public string? Website { get; set; }

		[DataMember(Name = "publicly_traded")]
		public string? PubliclyTraded { get; set; }

		[DataMember(Name = "sys_mod_count")]
		public string? SysModCount { get; set; }

		[DataMember(Name = "sys_tags")]
		public string? SysTags { get; set; }

		[DataMember(Name = "fax_phone")]
		public string? FaxPhone { get; set; }

		[DataMember(Name = "phone")]
		public string? Phone { get; set; }

		[DataMember(Name = "vendor_manager")]
		public string? VendorManager { get; set; }

		[DataMember(Name = "banner_text")]
		public string? BannerText { get; set; }

		[DataMember(Name = "name")]
		public string? Name { get; set; }

		[DataMember(Name = "customer")]
		public string? Customer { get; set; }

		[DataMember(Name = "primary")]
		public string? Primary { get; set; }
	}
}
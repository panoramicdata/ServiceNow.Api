using ServiceNow.Api.Attributes;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables
{
	[DataContract]
	[TableName("cmdb_ci_printer")]
	public class Printer : Hardware
	{
		[DataMember(Name = "resolution_units")]
		public string? ResolutionUnits { get; set; }

		[DataMember(Name = "hardware_substatus")]
		public string? HardwareSubstatus { get; set; }

		[DataMember(Name = "horizontal_resolution")]
		public string? HorizontalResolution { get; set; }

		[DataMember(Name = "use_units")]
		public string? UseUnits { get; set; }

		[DataMember(Name = "vertical_resolution")]
		public string? VerticalResolution { get; set; }

		[DataMember(Name = "color")]
		public string? Color { get; set; }

		[DataMember(Name = "postscript")]
		public string? Postscript { get; set; }

		[DataMember(Name = "print_type")]
		public string? PrintType { get; set; }

		[DataMember(Name = "colors")]
		public string? Colors { get; set; }

		[DataMember(Name = "use_count")]
		public string? UseCount { get; set; }

		[DataMember(Name = "paper")]
		public string? Paper { get; set; }

		[DataMember(Name = "mac_address")]
		public string? MacAddress { get; set; }

		[DataMember(Name = "pcl")]
		public string? Pcl { get; set; }

		[DataMember(Name = "ppm")]
		public string? Ppm { get; set; }

		[DataMember(Name = "sys_tags")]
		public string? SysTags { get; set; }
	}
}
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables
{
	[DataContract]
	public class ResourceLink<T>
	{
		[DataMember(Name = "link")]
		public string Link { get; set; } // REST URL for child record

		[DataMember(Name = "value")]
		public string Value { get; set; } // Reference to the child record (sys_id)

		// The ToString Method is called on serialization back to servicenow through the ResourceLinkConverter
		public override string ToString() => Value;
	}
}
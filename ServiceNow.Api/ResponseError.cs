using System.Runtime.Serialization;

namespace ServiceNow.Api
{
	[DataContract]
	public class ResponseError
	{
		[DataMember(Name = "message")]
		public string? Message { get; set; }

		[DataMember(Name = "detail")]
		public string? Detail { get; set; }
	}
}
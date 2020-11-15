using System.Runtime.Serialization;

namespace ServiceNow.Api
{
	[DataContract]
	public abstract class RestListResponseBase
	{
		public int TotalCount { get; set; }

		[DataMember(Name = "error")]
		public ResponseError? Error { get; set; }

		[DataMember(Name = "status")]
		public string? Status { get; set; }
	}
}
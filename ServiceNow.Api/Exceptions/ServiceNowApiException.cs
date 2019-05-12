using System;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Exceptions
{
	public class ServiceNowApiException : Exception
	{
		public ServiceNowApiException(string message) : base(message)
		{
		}

		public ServiceNowApiException(string message, Exception exception) : base(message, exception)
		{
		}

		public ServiceNowApiException() : base()
		{
		}

		protected ServiceNowApiException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
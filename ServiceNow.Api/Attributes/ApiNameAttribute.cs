using System;

namespace ServiceNow.Api.Attributes
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class ApiNameAttribute : Attribute
	{
		public string ApiName { get; }

		public ApiNameAttribute(string apiName) => ApiName = apiName;
	}
}
using System;

namespace ServiceNow.Api.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ApiNameAttribute : Attribute
{
	public ApiNameAttribute(string apiName)
	{
		ApiName = apiName;
	}

	public string ApiName { get; }
}
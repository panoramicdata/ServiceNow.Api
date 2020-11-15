using ServiceNow.Api.Attributes;
using System;
using System.Runtime.Serialization;

namespace ServiceNow.Api
{
	/// <summary>
	/// All things in ServiceNow are a CmdbEntity
	/// </summary>
	[DataContract]
	public abstract class CmdbEntity
	{
		[DataMember(Name = "sys_id")]
		public string SysId { get; set; } = null!;

		public static string? GetApiName<T>() where T : CmdbEntity
			=> ((ApiNameAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(ApiNameAttribute)))?.ApiName;
	}
}
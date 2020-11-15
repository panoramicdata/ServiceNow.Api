using ServiceNow.Api.Attributes;
using System;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables
{
	/// <summary>
	///    Base table for a service now record.  All table API records should include this at minimum.
	/// </summary>
	[ApiName("table")]
	[DataContract]
	public abstract class Table : CmdbEntity
	{
		// We potentially don't want to serialize sys_id because if we have it, we should always use it in the table path rather than putting it into a query.  Also, if we are performing a POST we won't have a valid sys_id anyway.
		// Have removed this until it becomes a problem
		//public bool ShouldSerializeSysId()
		//{
		//	return false;
		//}

		public static string GetTableName<T>() where T : Table
			=> ((TableNameAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(TableNameAttribute)))?.TableName ?? string.Empty;
	}
}
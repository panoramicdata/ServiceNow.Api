using ServiceNow.Api.Attributes;
using System;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables;

/// <summary>
///    Base table for a service now record.  All table API records should include this at minimum.
/// </summary>
[ApiName("table")]
[DataContract]
public abstract class Table : CmdbEntity
{
	public static string GetTableName<T>() where T : Table
		=> ((TableNameAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(TableNameAttribute)))?.TableName ?? string.Empty;
}
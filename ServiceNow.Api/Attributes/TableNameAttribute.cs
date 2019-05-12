using System;

namespace ServiceNow.Api.Attributes
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class TableNameAttribute : Attribute
	{
		public TableNameAttribute(string tableName) => TableName = tableName;

		public string TableName { get; }
	}
}
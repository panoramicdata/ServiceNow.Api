using ServiceNow.Api.Attributes;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Tables;

/// <summary>
/// sys_calendar: a ServiceNow calendar record. Inherits the base <see cref="Table"/> fields (e.g. <see cref="CmdbEntity.SysId"/>).
/// </summary>
[DataContract]
[TableName("sys_calendar")]
public class SysCalendar : Table
{
}

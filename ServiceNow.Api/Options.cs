using Microsoft.Extensions.Logging;

namespace ServiceNow.Api
{
	/// <summary>
	/// Options
	/// </summary>
	public class Options
	{
		/// <summary>
		/// The logger
		/// </summary>
		public ILogger? Logger { get; set; }

		/// <summary>
		/// The field that is used in the "GetAllByQueryInternalAsync" method in ServiceNowClient.
		/// This can be set because it CAN happen that the table/view does NOT have this field,
		/// but another field e.g. inc_sys_created_on. Note it must be DateTime-related to parse properly
		/// </summary>
		public string PagingFieldName { get; set; } = "sys_created_on";

		/// <summary>
		/// Checks that the number of items returned matches the header total when getting all CIs
		/// </summary>
		public bool ValidateCountItemsReturned { get; set; } = true;

		/// <summary>
		/// The number of entries + or - that is acceptable difference from the total value returned by the ServiceNow API in the header
		/// </summary>
		public int ValidateCountItemsReturnedTolerance { get; set; } = 0;

		/// <summary>
		/// The default paging size
		/// </summary>
		public int PageSize { get; set; } = 1000;
	}
}
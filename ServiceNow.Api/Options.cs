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
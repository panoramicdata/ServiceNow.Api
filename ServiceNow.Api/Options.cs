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
		public ILogger Logger { get; set; }

		/// <summary>
		/// Checks that the number of items returned matches the header total when getting all CIs
		/// </summary>
		public bool ValidateCountItemsReturned { get; set; } = true;
	}
}
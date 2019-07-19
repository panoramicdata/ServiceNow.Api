namespace ServiceNow.Api.DiagCli.Models
{
	public class DiagnosticTest
	{
		public DiagnosticType Type { get; set; }

		public int? PageSize { get; set; } = 1000;

		public string Table { get; set; }

		public string Query { get; set; }
	}
}
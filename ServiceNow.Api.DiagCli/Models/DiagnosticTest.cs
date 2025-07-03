using System.Collections.Generic;

namespace ServiceNow.Api.DiagCli.Models;

public class DiagnosticTest
{
	public DiagnosticType Type { get; set; }

	public int PageSize { get; set; } = 1000;

	public string? Table { get; set; }

	public string? Query { get; set; }

	public List<string> Fields { get; set; } = ["sys_id"];
}
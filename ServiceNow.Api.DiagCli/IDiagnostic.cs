using ServiceNow.Api.DiagCli.Models;

namespace ServiceNow.Api.DiagCli;

internal interface IDiagnostic
{
	Task ExecuteAsync(DiagnosticTest test);
}
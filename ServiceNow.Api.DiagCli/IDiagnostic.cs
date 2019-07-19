using ServiceNow.Api.DiagCli.Models;
using System.Threading.Tasks;

namespace ServiceNow.Api.DiagCli
{
	internal interface IDiagnostic
	{
		Task ExecuteAsync(DiagnosticTest test);
	}
}
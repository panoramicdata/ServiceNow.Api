using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceNow.Api.DiagCli.Exceptions;
using ServiceNow.Api.DiagCli.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ServiceNow.Api.DiagCli;

internal class DiagApplication(
	ILogger<DiagApplication> logger,
	IOptions<Configuration> options,
	IServiceProvider serviceProvider)
{
	private readonly Configuration _configuration = options.Value;

	public async Task<int> RunAsync()
	{
		logger.LogInformation($"ServiceNow API Diagnostics v{ThisAssembly.AssemblyFileVersion}");

		try
		{
			_configuration.Validate();
		}
		catch (ConfigurationException ex)
		{
			logger.LogError(ex, "{Message}", ex.Message);
			return ExitCode.ConfigurationError;
		}

		var overallStopWatch = Stopwatch.StartNew();
		logger.LogInformation("Starting run...");

		ArgumentNullException.ThrowIfNull(_configuration.Tests);

		foreach (var test in _configuration.Tests)
		{
			await ExecuteTestAsync(test).ConfigureAwait(false);
		}

		logger.LogInformation("Run complete after {TotalSeconds:0.00}s.", overallStopWatch.Elapsed.TotalSeconds);
		return ExitCode.Success;
	}

	private async Task ExecuteTestAsync(DiagnosticTest test)
	{
		IDiagnostic diagnostic = test.Type switch
		{
			DiagnosticType.Paging => serviceProvider.GetRequiredService<PagingDiagnostic>(),
			_ => throw new NotSupportedException($"Test type {test.Type} is not supported."),
		};
		await diagnostic.ExecuteAsync(test).ConfigureAwait(false);
	}
}
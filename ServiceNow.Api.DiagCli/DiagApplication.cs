using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceNow.Api.DiagCli.Exceptions;
using ServiceNow.Api.DiagCli.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ServiceNow.Api.DiagCli;

internal class DiagApplication
{
	private readonly ILogger<DiagApplication> _logger;
	private readonly Configuration _configuration;
	private readonly IServiceProvider _serviceProvider;

	public DiagApplication(
		ILogger<DiagApplication> logger,
		IOptions<Configuration> options,
		IServiceProvider serviceProvider)
	{
		_logger = logger;
		_configuration = options.Value;
		_serviceProvider = serviceProvider;
	}

	public async Task<int> RunAsync()
	{
		_logger.LogInformation($"ServiceNow API Diagnostics v{ThisAssembly.AssemblyFileVersion}");

		try
		{
			_configuration.Validate();
		}
		catch (ConfigurationException ex)
		{
			_logger.LogError(ex, "{Message}", ex.Message);
			return ExitCode.ConfigurationError;
		}

		var overallStopWatch = Stopwatch.StartNew();
		_logger.LogInformation("Starting run...");

		ArgumentNullException.ThrowIfNull(_configuration.Tests);

		foreach (var test in _configuration.Tests)
		{
			await ExecuteTestAsync(test).ConfigureAwait(false);
		}

		_logger.LogInformation("Run complete after {TotalSeconds:0.00}s.", overallStopWatch.Elapsed.TotalSeconds);
		return ExitCode.Success;
	}

	private async Task ExecuteTestAsync(DiagnosticTest test)
	{
		IDiagnostic diagnostic = test.Type switch
		{
			DiagnosticType.Paging => _serviceProvider.GetRequiredService<PagingDiagnostic>(),
			_ => throw new NotSupportedException($"Test type {test.Type} is not supported."),
		};
		await diagnostic.ExecuteAsync(test).ConfigureAwait(false);
	}
}
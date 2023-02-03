using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceNow.Api.DiagCli.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceNow.Api.DiagCli;

internal class PagingDiagnostic : IDiagnostic
{
	private readonly Configuration _configuration;
	private readonly ILogger<PagingDiagnostic> _logger;

	public PagingDiagnostic(IOptions<Configuration> options, ILogger<PagingDiagnostic> logger)
	{
		_configuration = options.Value;
		_logger = logger;
	}

	public async Task ExecuteAsync(DiagnosticTest test)
	{
		ArgumentNullException.ThrowIfNull(_configuration.Credentials);
		ArgumentException.ThrowIfNullOrEmpty(test.Table);

		_logger.LogInformation($"Starting {nameof(PagingDiagnostic)}");

		try
		{
			using var client = new ServiceNowClient(
				_configuration.Credentials.ServiceNowAccount,
				_configuration.Credentials.ServiceNowUsername,
				_configuration.Credentials.ServiceNowPassword,
				new Options { ValidateCountItemsReturned = true, ValidateCountItemsReturnedTolerance = 0, PageSize = test.PageSize, Logger = _logger });

			var results = await client.GetAllByQueryAsync(test.Table, test.Query, fieldList: test.Fields).ConfigureAwait(false);
			_logger.LogInformation("Got {ResultCount} results", results.Count);

			// Check for dupes
			var dupes = results.GroupBy(ci => ci["sys_id"]).Where(g => g.Count() > 1).Select(g => new { Id = g.First()["sys_id"], Count = g.Count() }).ToList();
			var unique = results.GroupBy(ci => ci["sys_id"]).Select(ci => ci.First()).ToList();

			_logger.LogInformation("Found {DupesCount} dupes - total retrieved = {ResultsCount} - unique = {UniqueCount}",
				dupes.Count, results.Count, unique.Count);
		}
		catch (Exception e)
		{
			_logger.LogError(e, "{Message}", e.Message);
		}
	}
}
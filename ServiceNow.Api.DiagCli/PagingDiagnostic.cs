using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceNow.Api.DiagCli.Exceptions;
using ServiceNow.Api.DiagCli.Models;
using System.Threading.Tasks;

namespace ServiceNow.Api.DiagCli
{
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
			_logger.LogInformation($"Starting {nameof(PagingDiagnostic)}");

			if (string.IsNullOrWhiteSpace(test.Table))
			{
				throw new ConfigurationException($"{nameof(test.Table)} must be set.");
			}

			using (var client = new ServiceNowClient(
				_configuration.Credentials.ServiceNowAccount,
				_configuration.Credentials.ServiceNowUsername,
				_configuration.Credentials.ServiceNowPassword,
				new Options { ValidateCountItemsReturned = true }))
			{
				var results = await client.GetAllByQueryAsync(test.Table).ConfigureAwait(false);
				_logger.LogInformation($"Got {results.Count} results");
			}
		}
	}
}
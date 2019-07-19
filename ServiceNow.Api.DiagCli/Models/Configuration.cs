using ServiceNow.Api.DiagCli.Exceptions;
using System.Collections.Generic;

namespace ServiceNow.Api.DiagCli.Models
{
	public class Configuration
	{
		public const string DefaultFilename = "appsettings.json";

		public List<DiagnosticTest> Tests { get; set; }

		public Credentials Credentials { get; set; }

		internal void Validate()
		{
			if (Tests == null || Tests.Count == 0)
			{
				throw new ConfigurationException("There must be at least one test defined");
			}

			if (Credentials == null)
			{
				throw new ConfigurationException($"{nameof(Credentials)} must be set.");
			}

			if (string.IsNullOrWhiteSpace(Credentials.ServiceNowAccount))
			{
				throw new ConfigurationException($"{nameof(Credentials)}.{nameof(Credentials.ServiceNowAccount)} must be set.");
			}

			if (string.IsNullOrWhiteSpace(Credentials.ServiceNowUsername))
			{
				throw new ConfigurationException($"{nameof(Credentials)}.{nameof(Credentials.ServiceNowUsername)} must be set.");
			}

			if (string.IsNullOrWhiteSpace(Credentials.ServiceNowPassword))
			{
				throw new ConfigurationException($"{nameof(Credentials)}.{nameof(Credentials.ServiceNowPassword)} must be set.");
			}
		}
	}
}

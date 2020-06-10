using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using Xunit.Abstractions;

namespace ServiceNow.Api.Test
{
	public abstract class ServiceNowTest
	{
		protected ILogger Logger { get; }

		/// <summary>
		///   Constructs a ServiceNowClient
		/// </summary>
		/// <param name="iTestOutputHelper"></param>
		/// <param name="appsettingsFilename"></param>
		/// <param name="options"></param>
		protected ServiceNowTest(ITestOutputHelper iTestOutputHelper, string appsettingsFilename = "appsettings.json", Options options = null)
		{
			options ??= new Options();
			Logger = iTestOutputHelper.BuildLogger();
			options.Logger = Logger;

			// Locate the configuration file path at the root of the test project, relative from where these assemblies were deployed
			var configurationJsonFilePath = Path.Combine(Path.GetDirectoryName(typeof(ServiceNowTest).GetTypeInfo().Assembly.Location), "../../..");
			var configurationRoot = new ConfigurationBuilder()
				.SetBasePath(configurationJsonFilePath)
				.AddJsonFile(appsettingsFilename, optional: false, reloadOnChange: false)
				.Build();
			var config = configurationRoot.Get<TestConfiguration>();
			if (string.IsNullOrWhiteSpace(config.ServiceNowAccount))
			{
				throw new Exception($"{nameof(TestConfiguration)}.{nameof(TestConfiguration.ServiceNowAccount)} must be set.");
			}

			if (string.IsNullOrWhiteSpace(config.ServiceNowUsername))
			{
				throw new Exception($"{nameof(TestConfiguration)}.{nameof(TestConfiguration.ServiceNowUsername)} must be set.");
			}

			if (string.IsNullOrWhiteSpace(config.ServiceNowPassword))
			{
				throw new Exception($"{nameof(TestConfiguration)}.{nameof(TestConfiguration.ServiceNowPassword)} must be set.");
			}

			Client = new ServiceNowClient(
				config.ServiceNowAccount,
				config.ServiceNowUsername,
				config.ServiceNowPassword,
				options
			);
		}

		/// <summary>
		///    the client used by the test
		/// </summary>
		protected ServiceNowClient Client { get; }
	}
}
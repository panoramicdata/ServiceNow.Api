using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;

namespace ServiceNow.Api.Test;

public abstract class ServiceNowTest
{
	protected ILogger Logger { get; }

	/// <summary>
	///   Constructs a ServiceNowClient
	/// </summary>
	/// <param name="iTestOutputHelper"></param>
	/// <param name="appsettingsFilename"></param>
	/// <param name="options"></param>
	protected ServiceNowTest(
		ILogger logger,
		string appsettingsFilename = "appsettings.json",
		Options? options = null)
	{
		options ??= new();
		Logger = logger;
		options.Logger = Logger;

		// Locate the configuration file path at the root of the test project, relative from where these assemblies were deployed
		var configurationJsonFilePath = Path.Combine(Path.GetDirectoryName(typeof(ServiceNowTest).GetTypeInfo().Assembly.Location) ?? string.Empty, "../../..");
		var configurationRoot = new ConfigurationBuilder()
			.SetBasePath(configurationJsonFilePath)
			.AddJsonFile(appsettingsFilename, optional: true, reloadOnChange: false)
			.AddUserSecrets<ServiceNowTest>()
			.Build();

		var config = new TestConfiguration
		{
			ServiceNowAccount = configurationRoot["ServiceNowAccount"],
			ServiceNowUsername = configurationRoot["ServiceNowUsername"],
			ServiceNowPassword = configurationRoot["ServiceNowPassword"],
			ServiceNowEnvironment = configurationRoot["ServiceNowEnvironment"]
		};
		if (string.IsNullOrWhiteSpace(config.ServiceNowAccount))
		{
			throw new InvalidOperationException($"{nameof(TestConfiguration)}.{nameof(TestConfiguration.ServiceNowAccount)} must be set.");
		}

		if (string.IsNullOrWhiteSpace(config.ServiceNowUsername))
		{
			throw new InvalidOperationException($"{nameof(TestConfiguration)}.{nameof(TestConfiguration.ServiceNowUsername)} must be set.");
		}

		if (string.IsNullOrWhiteSpace(config.ServiceNowPassword))
		{
			throw new InvalidOperationException($"{nameof(TestConfiguration)}.{nameof(TestConfiguration.ServiceNowPassword)} must be set.");
		}

		var environment = ServiceNowEnvironment.Community;
		_ = Enum.TryParse(config.ServiceNowEnvironment, true, out environment);

		options.Environment = environment;

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
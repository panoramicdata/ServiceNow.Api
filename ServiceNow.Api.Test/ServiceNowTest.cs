using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace ServiceNow.Api.Test;

public abstract class ServiceNowTest : TestBed<Fixture>
{
	protected ILogger Logger { get; }

	/// <summary>
	///   Constructs a ServiceNowClient
	/// </summary>
	/// <param name="iTestOutputHelper"></param>
	/// <param name="appsettingsFilename"></param>
	/// <param name="options"></param>
	protected ServiceNowTest(ITestOutputHelper testOutputHelper, Fixture fixture) : base(testOutputHelper, fixture)
	{
		ArgumentNullException.ThrowIfNull(testOutputHelper);
		ArgumentNullException.ThrowIfNull(fixture);

		var loggerFactory = fixture.GetService<ILoggerFactory>(testOutputHelper) ?? throw new InvalidOperationException("LoggerFactory is null");
		Logger = loggerFactory.CreateLogger(GetType());

		// TestConfiguration
		var testConfigurationOptions = fixture
			.GetService<IOptions<TestConfiguration>>(testOutputHelper)
			?? throw new InvalidOperationException("TestConfiguration is null");

		var options = testConfigurationOptions.Value;

		var environment = ServiceNowEnvironment.Community;
		_ = Enum.TryParse(options.ServiceNowEnvironment, true, out environment);

		Client = new ServiceNowClient(
			options.ServiceNowAccount ?? string.Empty,
			options.ServiceNowUsername ?? string.Empty,
			options.ServiceNowPassword ?? string.Empty,
			new Options
			{
				Logger = Logger,
				Environment = environment
			}
		);
	}

	/// <summary>
	///    the client used by the test
	/// </summary>
	protected ServiceNowClient Client { get; }
}
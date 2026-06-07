using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace ServiceNow.Api.Test;

/// <summary>
/// Test fixture for ServiceNow API tests.  This fixture is responsible for setting up the test configuration and any shared services that the tests may need.  The configuration is loaded from user secrets, which allows you to keep sensitive information like API credentials out of source control.  The fixture also sets up a scoped CancellationTokenSource that can be used in tests to ensure that any ongoing operations are cancelled if the test is cancelled (e.g. due to a timeout).  The services registered in this fixture will be available for injection into test classes that use this fixture.
/// </summary>
public class Fixture : TestBedFixture
{
	private IConfigurationRoot? _configuration;

	/// <summary>
	/// Adds services to the test dependency injection container.  This method is called by the test framework when setting up the test context.  In this implementation, we register a scoped CancellationTokenSource and configure the TestConfiguration options using the configuration loaded from user secrets.  The TestConfiguration class should have properties that match the keys in the configuration section "Config" in order to be populated correctly.
	/// </summary>
	/// <param name="services">The service collection to which services will be added.</param>
	/// <param name="configuration">The configuration to use for setting up services.</param>
	/// <exception cref="InvalidOperationException">Thrown if the configuration is null.</exception>
	protected override void AddServices(
		IServiceCollection services,
		IConfiguration? configuration)
	{
		if (_configuration is null)
		{
			throw new InvalidOperationException("Configuration is null");
		}

		services
			.AddScoped<CancellationTokenSource>()
			.Configure<TestConfiguration>(_configuration.GetSection("Config"));
	}

	protected override ValueTask DisposeAsyncCore()
		=> default;

	protected override IEnumerable<TestAppSettings> GetTestAppSettings()
	{
		_configuration = new ConfigurationBuilder()
			 .AddUserSecrets<Fixture>()
			 .Build();

		// This is not used
		return [
			new TestAppSettings
			{
				IsOptional = true,
				Filename = null,
			}
		];
	}
}

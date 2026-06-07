using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace ServiceNow.Api.Test;

/// <summary>
/// Base class for ServiceNow API tests.  This class sets up the ServiceNowClient and provides a Logger for use in tests.  It also provides a CancellationToken that is tied to the test context, so that if the test is cancelled (e.g. due to a timeout), any ongoing operations that respect the cancellation token will be cancelled as well.  The client is configured using options from the test configuration, which should be provided in the test fixture.  The logger is configured to write to the xUnit test output, which allows you to see log messages in the context of the test run.
/// </summary>
public abstract class ServiceNowTest : TestBed<Fixture>
{
	protected ILogger Logger { get; }

	protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	/// <summary>
	///   Constructs a ServiceNowClient
	/// </summary>
	/// <param name="testOutputHelper">The xUnit output helper for the current test.</param>
	/// <param name="fixture">The shared test fixture.</param>
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

		var environment = Enum.TryParse<ServiceNowEnvironment>(options.ServiceNowEnvironment, true, out var parsed)
			? parsed
			: ServiceNowEnvironment.Community;

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
	/// The client used by the test
	/// </summary>
	protected ServiceNowClient Client { get; }
}
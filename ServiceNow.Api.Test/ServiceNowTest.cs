using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace ServiceNow.Api.Test;

/// <summary>
/// Base class for ServiceNow API tests.  This class sets up the ServiceNowClient and provides a Logger for use in tests.  It also provides a CancellationToken that is tied to the test context, so that if the test is cancelled (e.g. due to a timeout), any ongoing operations that respect the cancellation token will be cancelled as well.  The client is configured using options from the test configuration, which should be provided in the test fixture.  The logger is configured to write to the xUnit test output, which allows you to see log messages in the context of the test run.
/// </summary>
[Trait("Category", "Integration")]
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

		Configuration = testConfigurationOptions.Value;

		// Fail fast with an actionable message rather than letting an empty account reach
		// the client, where it surfaces as "Invalid URI: The hostname could not be parsed".
		AssertConfigured(Configuration.ServiceNowAccount, nameof(TestConfiguration.ServiceNowAccount));
		AssertConfigured(Configuration.ServiceNowUsername, nameof(TestConfiguration.ServiceNowUsername));
		AssertConfigured(Configuration.ServiceNowPassword, nameof(TestConfiguration.ServiceNowPassword));

		Client = CreateClient(new Options
		{
			Logger = Logger,
			Environment = ConfiguredEnvironment
		});
	}

	private static void AssertConfigured(string? value, string name)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new InvalidOperationException(
				$"These tests require '{name}' in the ServiceNow.Api.Test user secrets. " +
				$"Set it with: dotnet user-secrets set \"{name}\" \"...\" " +
				$"--project ServiceNow.Api.Test. See userSecrets.example.json for the full shape.");
		}
	}

	/// <summary>
	/// The resolved test configuration, so that a test can build a client with bespoke options.
	/// </summary>
	protected TestConfiguration Configuration { get; }

	/// <summary>
	/// The configured ServiceNow environment.
	/// </summary>
	protected ServiceNowEnvironment ConfiguredEnvironment
		=> Enum.TryParse<ServiceNowEnvironment>(Configuration.ServiceNowEnvironment, true, out var parsed)
			? parsed
			: ServiceNowEnvironment.Community;

	/// <summary>
	/// Creates an additional client using the configured credentials, for tests that need
	/// options differing from the shared <see cref="Client"/>. The caller owns disposal.
	/// </summary>
	/// <param name="options">The options to construct the client with.</param>
	protected ServiceNowClient CreateClient(Options options)
		=> new(
			Configuration.ServiceNowAccount,
			Configuration.ServiceNowUsername,
			Configuration.ServiceNowPassword,
			options);

	/// <summary>
	/// The client used by the test
	/// </summary>
	protected ServiceNowClient Client { get; }
}
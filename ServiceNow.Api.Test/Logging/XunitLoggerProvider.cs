using Microsoft.Extensions.Logging;
using Xunit;

namespace ServiceNow.Api.Test.Logging;

/// <summary>
/// An ILoggerProvider that creates loggers which write to the xUnit test output
/// </summary>
/// <param name="output"></param>
/// <param name="minLevel"></param>
public class XunitLoggerProvider(ITestOutputHelper output, LogLevel minLevel) : ILoggerProvider
{
	private readonly ITestOutputHelper _output = output;
	private readonly LogLevel _minLevel = minLevel;

	/// <summary>
	/// Constructs an XunitLoggerProvider with a minimum log level of Trace
	/// </summary>
	/// <param name="output"></param>
	public XunitLoggerProvider(ITestOutputHelper output)
		: this(output, LogLevel.Trace)
	{
	}

	/// <summary>
	/// Creates a new XunitLogger with the specified category name
	/// </summary>
	/// <param name="categoryName"></param>
	/// <returns></returns>
	public ILogger CreateLogger(string categoryName)
		=> new XunitLogger(_output, categoryName, _minLevel);

	public void Dispose() => GC.SuppressFinalize(this);
}
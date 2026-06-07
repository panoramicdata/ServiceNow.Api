using Microsoft.Extensions.Logging;
using System.Text;
using Xunit;

namespace ServiceNow.Api.Test.Logging;

/// <summary>
/// An ILogger implementation that writes to xUnit's ITestOutputHelper.  This is really useful for seeing log output in the context of a test run, especially when running tests in parallel.  It is registered in the test fixture and can be used by requesting an ILogger&lt;T&gt; in the test class constructor
/// </summary>
/// <param name="output"></param>
/// <param name="category"></param>
/// <param name="minLogLevel"></param>
public class XunitLogger(ITestOutputHelper output, string category, LogLevel minLogLevel) : ILogger
{
	private static readonly string[] _newLineChars = [Environment.NewLine];
	private readonly string _category = category;
	private readonly LogLevel _minLogLevel = minLogLevel;
	private readonly ITestOutputHelper _output = output;

	/// <summary>
	/// Logs the specified message to the xUnit test output, if the log level is greater than or equal to the minimum log level specified in the constructor.  The message is buffered into a single string before being written to avoid shearing when running across multiple threads.  If an exception is provided, its ToString() will be included in the log output.
	/// </summary>
	/// <typeparam name="TState"></typeparam>
	/// <param name="logLevel"></param>
	/// <param name="eventId"></param>
	/// <param name="state"></param>
	/// <param name="exception"></param>
	/// <param name="formatter"></param>
	public void Log<TState>(
		LogLevel logLevel,
		EventId eventId,
		TState state,
		Exception? exception,
		Func<TState, Exception?, string> formatter)
	{
		if (!IsEnabled(logLevel))
		{
			return;
		}

		// Buffer the message into a single string in order to avoid shearing the message when running across multiple threads.
		var messageBuilder = new StringBuilder();

		var firstLinePrefix = $"| {_category} {logLevel}: ";
		var lines = formatter(state, exception).Split(_newLineChars, StringSplitOptions.RemoveEmptyEntries);
		_ = messageBuilder.AppendLine(firstLinePrefix + lines.FirstOrDefault() ?? string.Empty);

		var additionalLinePrefix = "|" + new string(' ', firstLinePrefix.Length - 1);
		foreach (var line in lines.Skip(1))
		{
			_ = messageBuilder.Append(additionalLinePrefix).AppendLine(line);
		}

		if (exception != null)
		{
			lines = exception.ToString().Split(_newLineChars, StringSplitOptions.RemoveEmptyEntries);
			additionalLinePrefix = "| ";
			foreach (var line in lines.Skip(1))
			{
				_ = messageBuilder.Append(additionalLinePrefix).AppendLine(line);
			}
		}

		// Remove the last line-break, because ITestOutputHelper only has WriteLine.
		var message = messageBuilder.ToString();
		if (message.EndsWith(Environment.NewLine, StringComparison.Ordinal))
		{
			message = message[..^Environment.NewLine.Length];
		}

		try
		{
			_output.WriteLine(message);
		}
		catch
		{
			// We could fail because we're on a background thread and our captured ITestOutputHelper is
			// busted (if the test "completed" before the background thread fired).
			// So, ignore this. There isn't really anything we can do but hope the
			// caller has additional loggers registered
		}
	}

	/// <summary>
	/// Determines whether the specified log level is enabled, based on the minimum log level specified in the constructor.
	/// </summary>
	/// <param name="logLevel"></param>
	/// <returns></returns>
	public bool IsEnabled(LogLevel logLevel)
		=> logLevel >= _minLogLevel;

	/// <summary>
	/// Begins a logical operation scope.  This logger does not support scopes, so this method returns a disposable that does nothing.
	/// </summary>
	public IDisposable BeginScope<TState>(TState state) where TState : notnull
		=> new NullScope();

	private sealed class NullScope : IDisposable
	{
		public void Dispose()
		{
		}
	}
}

/// <summary>
/// An ILogger implementation that writes to xUnit's ITestOutputHelper.  This is really useful for seeing log output in the context of a test run, especially when running tests in parallel.  It is registered in the test fixture and can be used by requesting an ILogger&lt;T&gt; in the test class constructor
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="output"></param>
public class XunitLogger<T>(ITestOutputHelper output) : ILogger<T>, IDisposable
{
	private readonly ITestOutputHelper _output = output;
	private bool _disposedValue;

	/// <summary>
	/// Logs the specified message to the xUnit test output.
	/// </summary>
	/// <typeparam name="TState"></typeparam>
	/// <param name="logLevel"></param>
	/// <param name="eventId"></param>
	/// <param name="state"></param>
	/// <param name="exception"></param>
	/// <param name="formatter"></param>
	public void Log<TState>(
		LogLevel logLevel,
		EventId eventId,
		TState state,
		Exception? exception,
		Func<TState, Exception?, string> formatter)
		=> _output.WriteLine(state?.ToString() ?? string.Empty);

	/// <summary>
	/// Determines whether the specified log level is enabled.  This implementation always returns true, but you could modify this to filter log levels if desired.
	/// </summary>
	/// <param name="logLevel"></param>
	/// <returns></returns>
	public bool IsEnabled(LogLevel logLevel) => true;

	/// <summary>
	/// Begins a logical operation scope.  This logger does not support scopes, so this methodreturns a disposable that does nothing.
	/// </summary>
	public IDisposable BeginScope<TState>(TState state) where TState : notnull => this;

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			_disposedValue = true;
		}
	}

	/// <summary>
	/// Disposes the logger.  Since this logger does not have any unmanaged resources, this just sets a flag to prevent future logging and suppresses finalization.
	/// </summary>
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
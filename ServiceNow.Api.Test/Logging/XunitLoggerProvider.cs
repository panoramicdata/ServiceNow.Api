using Microsoft.Extensions.Logging;
using System;
using Xunit.Abstractions;

namespace LogicMonitor.Api.Test.Logging;

public class XunitLoggerProvider(ITestOutputHelper output, LogLevel minLevel) : ILoggerProvider
{
	private readonly ITestOutputHelper _output = output;
	private readonly LogLevel _minLevel = minLevel;

	public XunitLoggerProvider(ITestOutputHelper output)
		: this(output, LogLevel.Trace)
	{
	}

	public ILogger CreateLogger(string categoryName)
		=> new XunitLogger(_output, categoryName, _minLevel);

	public void Dispose() => GC.SuppressFinalize(this);
}
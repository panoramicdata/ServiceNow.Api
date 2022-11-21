using Microsoft.Extensions.Logging;
using System;
using Xunit.Abstractions;

namespace LogicMonitor.Api.Test.Logging;

public class XunitLoggerProvider : ILoggerProvider
{
	private readonly ITestOutputHelper _output;
	private readonly LogLevel _minLevel;

	public XunitLoggerProvider(ITestOutputHelper output)
		: this(output, LogLevel.Trace)
	{
	}

	public XunitLoggerProvider(ITestOutputHelper output, LogLevel minLevel)
	{
		_output = output;
		_minLevel = minLevel;
	}

	public ILogger CreateLogger(string categoryName)
		=> new XunitLogger(_output, categoryName, _minLevel);

	public void Dispose() => GC.SuppressFinalize(this);
}
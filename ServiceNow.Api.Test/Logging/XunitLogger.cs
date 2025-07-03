﻿using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using Xunit.Abstractions;

namespace LogicMonitor.Api.Test.Logging;

public class XunitLogger(ITestOutputHelper output, string category, LogLevel minLogLevel) : ILogger
{
	private static readonly string[] _newLineChars = new[] { Environment.NewLine };
	private readonly string _category = category;
	private readonly LogLevel _minLogLevel = minLogLevel;
	private readonly ITestOutputHelper _output = output;

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
		if (message.EndsWith(Environment.NewLine))
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

	public bool IsEnabled(LogLevel logLevel)
		=> logLevel >= _minLogLevel;

	public IDisposable BeginScope<TState>(TState state) where TState : notnull
		=> new NullScope();

	private class NullScope : IDisposable
	{
		public void Dispose()
		{
		}
	}
}

public class XunitLogger<T>(ITestOutputHelper output) : ILogger<T>, IDisposable
{
	private readonly ITestOutputHelper _output = output;
	private bool _disposedValue;

	public void Log<TState>(
		LogLevel logLevel,
		EventId eventId,
		TState state,
		Exception? exception,
		Func<TState, Exception?, string> formatter)
		=> _output.WriteLine(state?.ToString() ?? string.Empty);

	public bool IsEnabled(LogLevel logLevel) => true;

	public IDisposable BeginScope<TState>(TState state) where TState : notnull => this;

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
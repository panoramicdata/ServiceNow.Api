using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace LogicMonitor.Api.Test.Logging;

 public static class XunitLoggerProviderStatics
 {
	  public static ILoggerFactory AddXunit(this ILoggerFactory factory, ITestOutputHelper output, LogLevel minLogLevel)
	  {
			factory.AddProvider(new XunitLoggerProvider(output, minLogLevel));
			return factory;
	  }
 }
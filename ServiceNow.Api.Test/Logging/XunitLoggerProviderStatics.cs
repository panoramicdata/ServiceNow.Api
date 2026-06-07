using Microsoft.Extensions.Logging;
using Xunit;

namespace ServiceNow.Api.Test.Logging;

 public static class XunitLoggerProviderStatics
 {
	  public static ILoggerFactory AddXunit(this ILoggerFactory factory, ITestOutputHelper output, LogLevel minLogLevel)
	  {
			factory.AddProvider(new XunitLoggerProvider(output, minLogLevel));
			return factory;
	  }
 }
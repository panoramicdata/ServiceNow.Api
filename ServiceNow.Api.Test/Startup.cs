using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.DependencyInjection.Logging;

namespace ServiceNow.Api.Test;

public class Startup
{
	public static void ConfigureServices(IServiceCollection services)
	{
		var config = new ConfigurationBuilder()
			.AddUserSecrets<Startup>()
			.Build();

		_ = services
			.AddLogging(lb => lb
				.AddDebug()
				.AddFilter(level => level >= LogLevel.Debug)
				.AddXunitOutput()
			)
			;
	}
}

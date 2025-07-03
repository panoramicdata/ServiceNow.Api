using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace ServiceNow.Api.Test;

public class Fixture : TestBedFixture
{
	private IConfigurationRoot? _configuration;

	protected override void AddServices(
		IServiceCollection services,
		IConfiguration? configuration)
	{
		if (_configuration is null)
		{
			throw new InvalidOperationException("Configuration is null");
		}

		services
			.AddScoped<CancellationTokenSource>()
			.Configure<TestConfiguration>(_configuration.GetSection("Config"));
	}

	protected override ValueTask DisposeAsyncCore()
		=> default;

	protected override IEnumerable<TestAppSettings> GetTestAppSettings()
	{
		_configuration = new ConfigurationBuilder()
			 .AddUserSecrets<Fixture>()
			 .Build();

		// This is not used
		return [
			new TestAppSettings
			{
				IsOptional = true,
				Filename = null,
			}
		];
	}
}

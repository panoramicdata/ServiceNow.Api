using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ServiceNow.Api.DiagCli.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ServiceNow.Api.DiagCli;

internal static class Program
{
	private static async Task<int> Main(string[] args)
	{
		Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
		try
		{
			var services = ConfigureServices();
			var configuration = new ConfigurationBuilder();
			BuildConfig(configuration, args);
			_ = services.AddOptions();
			_ = services.Configure<Configuration>(configuration.Build());

			var serviceProvider = services.BuildServiceProvider()
				?? throw new Exception("Could not build ServiceProvider");

			return await serviceProvider.GetRequiredService<DiagApplication>().RunAsync().ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "{Message}", ex.Message);
			return ExitCode.Error;
		}
		finally
		{
			Log.CloseAndFlush();
		}
	}

	private static IServiceCollection ConfigureServices()
	{
		IServiceCollection services = new ServiceCollection();

		_ = services.AddTransient<DiagApplication>();
		_ = services.AddTransient<PagingDiagnostic>();

		_ = services.AddLogging(loggingBuilder =>
			loggingBuilder.AddSerilog(
				new LoggerConfiguration()
					.MinimumLevel.Verbose()
					.WriteTo.Console()
					.WriteTo.File($"Log-{DateTimeOffset.UtcNow:yyyyMMddTHHmmssZ}.txt")
					.CreateLogger(),
				dispose: true));

		return services;
	}

	internal static void BuildConfig(IConfigurationBuilder configurationBuilder, string[] args)
	{
		var appsettingsFilename = Configuration.DefaultFilename;
		// If specifying any command line arguments, the first argument must be the path to the appsettings.json file ConfigFile: xxx
		if (args.Length > 0)
		{
			appsettingsFilename = args[0];
		}
		// Convert appsettingsFilename to absolute path for the ConfigurationBuilder to be able to find it
		appsettingsFilename = Path.GetFullPath(appsettingsFilename);

		_ = configurationBuilder
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile(appsettingsFilename, false, false);
	}
}

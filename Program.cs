using TwitchBot.Services;
using TwitchBot.Models;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TwitchBot
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var host = CreateHostBuilder(args);

            await host.RunAsync();
            return Environment.ExitCode;
        }

        static IHost CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    configuration.Sources.Clear();

                    IHostEnvironment env = hostingContext.HostingEnvironment;

                    configuration
                        .AddJsonFile("appsettings.json", true, true);

                    IConfigurationRoot configurationRoot = configuration.Build();

                    TwitchSettings options = new();
                    configurationRoot.GetSection(nameof(TwitchSettings))
                                     .Bind(options);
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services.AddSingleton<ITwitchService, TwitchService>();
                    services.AddHostedService<Worker>();

                    var settings = hostingContext.Configuration
                        .GetSection(nameof(TwitchSettings))
                        .Get<TwitchSettings>();
                    services.AddSingleton<TwitchSettings>(settings);
                }).Build();
    }
}

using TwitchBot.Services;
using TwitchBot.Models;
using TwitchBot.Models.Memory;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Text.Json;
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
                        .AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile("state.json", true, true);

                    IConfigurationRoot configurationRoot = configuration.Build();

                    TwitchSettings settings = new();
                    configurationRoot.GetSection(nameof(TwitchSettings))
                                     .Bind(settings);
                    
                    Memory memory = new();
                    configurationRoot.GetSection(nameof(Memory))
                                     .Bind(memory);
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services.AddSingleton<ITwitchService, TwitchService>();
                    services.AddSingleton<ICommandService, CommandService>();
                    services.AddHostedService<Worker>();

                    // Add settings for IRC connection
                    var settings = hostingContext.Configuration
                        .GetSection(nameof(TwitchSettings))
                        .Get<TwitchSettings>();

                    services.AddSingleton<TwitchSettings>(settings);
                    
                    // Add memory information for persistent state
                    var memory = hostingContext.Configuration
                        .GetSection(nameof(Memory))
                        .Get<Memory>();

                    services.AddSingleton<Memory>(memory);
                }).Build();
    }
}

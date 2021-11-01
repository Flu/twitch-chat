using System;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Threading;
using TwitchBot.Models;
using TwitchBot.Services;

namespace TwitchBot
{
    public class Worker : IHostedService
    {
        private readonly ITwitchService _twitchService;

        public Worker(ITwitchService twitchService)
        {
            _twitchService = twitchService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _twitchService.OnMessage += OnMessageCallback;
            _twitchService.StartListening("squishymuffinz");
            return Task.CompletedTask;
        }

        public void OnMessageCallback(object sender, TwitchMessageEventArgs args)
        {
            Console.WriteLine($"\x1b[1m{args.User}\x1b[0m: {args.Message}");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Maybe do some clean-up if needed
            return Task.CompletedTask;
        }
    }
}
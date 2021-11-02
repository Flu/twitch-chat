using System;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
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

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _twitchService.OnMessage += OnMessageCallback;
            await _twitchService.StartListening("hiko");
        }

        public string ColoredBoldName(string name)
        {
            using (var sha = new SHA256Managed())
            {
                byte[] textBytes = Encoding.UTF8.GetBytes(name);
                byte[] hashBytes = sha.ComputeHash(textBytes);

                var i = BitConverter.ToUInt64(hashBytes, 0) % 7 + 1;

                return $"\x1b[3{i}m\x1b[1m{name}\x1b[0m\x1b[39m";
            }
        }

        public void OnMessageCallback(object sender, TwitchMessageEventArgs args)
        {
            Console.WriteLine($"{ColoredBoldName(args.User)}: {args.Message}");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // Maybe do some clean-up if needed
        }
    }
}
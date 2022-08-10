using TwitchBot.Services;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Security.Cryptography;
using System;
using Microsoft.Extensions.Hosting;

namespace TwitchBot
{
    public class Worker : BackgroundService
    {
        private readonly ITwitchService _twitchService;

        public Worker(ITwitchService twitchService)
        {
            _twitchService = twitchService;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _twitchService.OnMessage += OnMessageCallback;
            await _twitchService.StartListening("westworld", cancellationToken);
        }

        public string ColoredBoldName(string name)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] textBytes = Encoding.UTF8.GetBytes(name);
                byte[] hashBytes = sha.ComputeHash(textBytes);

                var i = BitConverter.ToUInt64(hashBytes, 0) % 7 + 1;

                return $"\x1b[3{i}m\x1b[1m{name}\x1b[0m\x1b[39m";
            }
        }

        public string HighlightReference(string messageBody, string selfUsername, string sender)
        {
            if (messageBody.Contains(selfUsername) || sender == selfUsername)
            {
                return $"\x1b[47m\x1b[30m{messageBody}\x1b[0m";
            }
            else
            {
                return messageBody;
            }
        }

        public async Task OnMessageCallback(object sender, TwitchMessageEventArgs args)
        {
            Console.WriteLine($"{ColoredBoldName(args.Sender)}: {HighlightReference(args.Message, args.User, args.Sender)}");
            if (args.Message.ToLower().StartsWith("orangeflu: analysis"))
            {
                await _twitchService.SendMessage("[DBG] Analysis mode activated, awaiting prompt");
            }
        }
    }
}
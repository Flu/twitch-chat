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
        private readonly ICommandService _commandService;

        public Worker(ITwitchService twitchService, ICommandService commandService)
        {
            _twitchService = twitchService;
            _commandService = commandService;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _twitchService.OnMessage += OnMessageCallback;
            await _twitchService.StartListening("romanian", cancellationToken);
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

        // check if user is mentioned
        private bool IsMentioned(string message, string selfUsername)
        {   
            return message.Contains($"{selfUsername}");
        }

        public string HighlightReference(string messageBody, string selfUsername, string sender)
        {
            if (IsMentioned(messageBody, selfUsername) || sender == selfUsername)
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
            var response = await _commandService.ProcessCommand(args.Message, args.Sender);
            if (response != null)
            {
                await _twitchService.SendMessage(response);
            }
        }
    }
}
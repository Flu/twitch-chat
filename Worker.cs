using TwitchBot.Services;
using TwitchBot.Models.Memory;
using TwitchBot.Extensions;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Security.Cryptography;
using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace TwitchBot
{
    public class Worker : BackgroundService
    {
        private readonly ITwitchService _twitchService;
        private readonly ICommandService _commandService;
        private readonly Memory _memory;

        public Worker(ITwitchService twitchService, ICommandService commandService, Memory memory)
        {
            _twitchService = twitchService;
            _commandService = commandService;
            _memory = memory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _twitchService.OnMessage += OnMessageCallback;
            _twitchService.OnConnected += OnConnectedCallback;

            await _twitchService.StartListening(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await SaveHostData();
        }

        private async Task SaveHostData() {
            _memory.TrialNumber += 1;
            _memory.LastOnline = DateTime.Now;

            string jsonString = JsonSerializer.Serialize(new { memory = _memory });
            Console.WriteLine(jsonString);
            await File.WriteAllTextAsync("state.json", jsonString);
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
            var response = await _commandService.ProcessCommand(args);
            if (response != null)
            {
                await _twitchService.SendMessage(args.Channel, response);
            }
        }

        public async Task OnConnectedCallback(object sender, TwitchMessageEventArgs args)
        {
            var duration = (DateTime.Now - _memory.LastOnline).ToReadableString();

            await _twitchService.SendMessage(args.Channel, $"[Analysis mode for host {_memory.HostId}]");
            await _twitchService.SendMessage(args.Channel, $"[DBG] Current UNIX time: {DateTimeOffset.Now.ToUnixTimeSeconds()}");
            await _twitchService.SendMessage(args.Channel, $"[DBG] {duration} since the unit was last online");
            await _twitchService.SendMessage(args.Channel, $"[DBG] Trial number {_memory.TrialNumber}");
        }
    }
}

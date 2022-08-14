using TwitchBot.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Catalyst;
using Catalyst.Models;
using Mosaik.Core;
using Version = Mosaik.Core.Version;

namespace TwitchBot.Services
{
    public class CommandState
    {
        public bool isInAnalysisMode = false;

        public string TurnAnalysisMode(bool on)
        {
            this.isInAnalysisMode = on;
            return $"[DBG] Analysis mode {(this.isInAnalysisMode ? "activated" : "deactivated")}";
        }
    }

    public class CommandService : ICommandService
    {

        private readonly ILogger<CommandService> _logger;
        private readonly TwitchSettings _settings;
        // Map of channel name string to CommandState
        private IDictionary<string, CommandState> _commandStates;


        public CommandService(ILogger<CommandService> logger, TwitchSettings settings)
        {
            _logger = logger;
            _settings = settings;
            _commandStates = new Dictionary<string, CommandState>();
        }

        // Get a command and process it
        public async Task<string> ProcessCommand(TwitchMessageEventArgs args)
        {
            var command = args.Message;
            var user = args.User;
            var channel = args.Channel;
            var state = GetCommandState(channel);

            // Match the command through regexes and dispatch it through the command handler
            var response = command switch
            {
                "analysis" => state.TurnAnalysisMode(true), // built-in commnad
                "resume" => state.TurnAnalysisMode(false), // built-in command
                _ => (state.isInAnalysisMode ? await ProcessNormalCommand(args) : "") // normal command
            };

            return response;
        }

        // Get the command state for a channel
        private CommandState GetCommandState(string channel)
        {
            if (!_commandStates.ContainsKey(channel))
            {
                _commandStates.Add(channel, new CommandState());
            }
            return _commandStates[channel];
        }

        private string HelloCommand(string user)
        {
            return $"{user} : Hello!";
        }

        private async Task<string> ProcessNormalCommand(TwitchMessageEventArgs args)
        {
            string command = args.Message;
            string user = args.Sender;
            string channel = args.Channel;
            _logger.LogInformation("Downloading/reading language detection models");
            const string modelFolderName = "catalyst-models";

            if (!new DirectoryInfo(modelFolderName).Exists)
            {
                _logger.LogInformation("Downloading for the first time, may take a while");
            }

            Catalyst.Models.English.Register();
            Storage.Current = new DiskStorage(modelFolderName);
            var nlp = await Pipeline.ForAsync(Language.English);

            var doc = new Document(command);
            var langDetector = await LanguageDetector.FromStoreAsync(Language.Any, Version.Latest, "");

            langDetector.Process(doc);
            nlp.ProcessSingle(doc);

            _logger.LogInformation($"{doc.ToJson()}");
            return $"{user}: This is {doc.Language}, structure {doc.ToJson()}";
        }
    }
}

using TwitchBot.Models;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using Catalyst;
using Catalyst.Models;
using Mosaik.Core;
using Version = Mosaik.Core.Version;

namespace TwitchBot.Services {
    public class CommandService : ICommandService {

        private readonly ILogger<CommandService> _logger;
        private readonly TwitchSettings _settings;
        private bool isInAnalysisMode = false;


        public CommandService(ILogger<CommandService> logger, TwitchSettings settings) {
            _logger = logger;
            _settings = settings;
        }
        
        // Get a command and process it
        public async Task<string> ProcessCommand(string command, string user) {

            // Match the command through regexes and dispatch it through the command handler
            var response = command switch {
                "analysis" => TurnAnalysisMode(true), // built-in commnad
                "resume" => TurnAnalysisMode(false), // built-in command
                _ => (isInAnalysisMode ? await ProcessNormalCommand(command, user) : "") // normal command
            };

            return response;
        }

        private string HelloCommand(string user) {
            return $"{user} : Hello!";
        }

        private string TurnAnalysisMode(bool on) {
            this.isInAnalysisMode = on;
            return $"[DBG] Analysis mode {(this.isInAnalysisMode ? "activated" : "deactivated")}";
        }

        private async Task<string> ProcessNormalCommand(string command, string user) {
            _logger.LogInformation("Downloading/reading language detection models");
            const string modelFolderName = "catalyst-models";

            if (!new DirectoryInfo(modelFolderName).Exists) {
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
using System.Threading.Tasks;
using TwitchBot.Models;
using Microsoft.Extensions.Logging;

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
        public Task<string?> ProcessCommand(string command, string user) {

            // Match the command through regexes and dispatch it through the command handler
            var response = command switch {
                "analysis" => Task.FromResult(TurnAnalysisMode(true)),
                "resume" => Task.FromResult(TurnAnalysisMode(false)),
                "!hello" => Task.FromResult(HelloCommand(user)),
                _ => Task.FromResult($"{user} : Unknown command")
            };

            if (isInAnalysisMode) {
                return response;
            } else {
                return Task.FromResult<string>(null);
            }
        }

        private string HelloCommand(string user) {
            return $"{user} : Hello!";
        }

        private string TurnAnalysisMode(bool on) {
            this.isInAnalysisMode = on;
            return $"[DBG] Analysis mode {(this.isInAnalysisMode ? "activated" : "deactivated")}";
        }
    }
}
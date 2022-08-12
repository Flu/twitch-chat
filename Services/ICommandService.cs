using System.Threading.Tasks;

namespace TwitchBot.Services {
    public interface ICommandService {
        
        // Get a command and process it
        Task<string> ProcessCommand(string command, string user);
    }
}
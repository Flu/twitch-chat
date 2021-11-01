using System.Threading.Tasks;

namespace TwitchBot.Services {
    public delegate void TwitchChatEventHandler(object sender, TwitchMessageEventArgs args);

    public interface ITwitchService {
        Task StartListening(string channelName);
        
        event TwitchChatEventHandler OnMessage;
    }
}
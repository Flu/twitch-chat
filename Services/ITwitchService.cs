using System.Threading.Tasks;
using System.Threading;

namespace TwitchBot.Services {
    public delegate void TwitchChatEventHandler(object sender, TwitchMessageEventArgs args);

    public interface ITwitchService {
        Task StartListening(string channelName, CancellationToken cancellationToken);
        
        event TwitchChatEventHandler OnMessage;
    }
}
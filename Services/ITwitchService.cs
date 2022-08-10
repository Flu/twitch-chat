using System.Threading.Tasks;
using System.Threading;

namespace TwitchBot.Services {
    public delegate Task TwitchChatEventHandler(object sender, TwitchMessageEventArgs args);

    public interface ITwitchService {
        Task StartListening(string channelName, CancellationToken cancellationToken);
        Task SendMessage(string message);
        event TwitchChatEventHandler OnMessage;
    }
}
using System.Threading.Tasks;
using System.Threading;
using System;

namespace TwitchBot.Services {
    public delegate Task TwitchChatEventHandler(object sender, TwitchMessageEventArgs args);
    public delegate Task TwitchChatConnectedEventHandler(object sender, TwitchMessageEventArgs args); 

    public interface ITwitchService {
        Task StartListening(CancellationToken cancellationToken);
        Task SendMessage(string channelName, string message);
        event TwitchChatEventHandler OnMessage;
        // create a new event handler for the OnConnected event
        event TwitchChatConnectedEventHandler OnConnected;
    }
}

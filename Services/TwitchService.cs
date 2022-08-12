using TwitchBot.Models;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Net.Sockets;
using System.Net.Security;
using System.IO;
using System;

namespace TwitchBot.Services
{
    public class TwitchMessageEventArgs : EventArgs
    {
        public string Message { get; set; }
        public string User { get; set; }
        public string Sender { get; set; }
    }

    public class TwitchService : ITwitchService
    {
        private readonly ILogger<TwitchService> _logger;
        private readonly TwitchSettings _settings;
        private string _channelName;
        private StreamWriter _streamWriter;
        private StreamReader _streamReader;
        public event TwitchChatEventHandler OnMessage;
        public event TwitchChatConnectedEventHandler OnConnected;

        public TwitchService(ILogger<TwitchService> logger, TwitchSettings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return sslPolicyErrors == SslPolicyErrors.None;
        }

        public async Task SendMessage(string message)
        {
            await _streamWriter.WriteLineAsync($"PRIVMSG #{_channelName} :{message}");
        }

        private async Task EstablishServerConnection()
        {
            var tcp = new TcpClient();

            await tcp.ConnectAsync(_settings.Url, _settings.Port);

            SslStream sslStream;

            sslStream = new SslStream(
                tcp.GetStream(),
                false,
                ValidateServerCertificate,
                null);

            await sslStream.AuthenticateAsClientAsync(_settings.Url);

            _streamReader = new StreamReader(_settings.Ssl ? sslStream : tcp.GetStream());
            _streamWriter = new StreamWriter(_settings.Ssl ? sslStream : tcp.GetStream())
            {
                NewLine = "\r\n",
                AutoFlush = true
            };

            await Login();
        }

        private async Task Login()
        {
            await _streamWriter.WriteLineAsync($"NICK {_settings.Credentials.Username}");

            await _streamReader.ReadLineAsync();
            await _streamReader.ReadLineAsync();

            // Get the ping :random_number
            string ping_line = await _streamReader.ReadLineAsync();
            string cookie = ping_line.Split(" ")[1];

            // Respond back with pong :random_number
            await _streamWriter.WriteLineAsync($"PONG {cookie}");
            await _streamWriter.WriteLineAsync($"USER {_settings.Credentials.Username} {_settings.Credentials.Username} {_settings.Credentials.Username} :{_settings.Credentials.Username}");
        }

        public async Task StartListening(string channelName, CancellationToken cancellationToken)
        {
            await EstablishServerConnection();
            _logger.LogInformation($"Authentication succesful to {_settings.Url}:{_settings.Port}");

            await _streamWriter.WriteLineAsync($"JOIN #{channelName}");
            _logger.LogInformation($"Joined channel {channelName}");
            _channelName = channelName;

            // Notify the worker that we are connected and ready to send messages
            await OnConnected.Invoke(this, null);

            while (!cancellationToken.IsCancellationRequested)
            {
                string line = await _streamReader.ReadLineAsync();
                string[] split = line.Split(" ");

                //PING :tmi.twitch.tv
                //Respond with PONG :tmi.twitch.tv
                if (line.StartsWith("PING"))
                {
                    await _streamWriter.WriteLineAsync($"PONG {split[1]}");
                }

                // Parse messages within the channel
                if (split.Length > 2 && split[1] == "PRIVMSG")
                {
                    //:mytwitchchannel!mytwitchchannel@mytwitchchannel.tmi.twitch.tv 
                    // ^^^^^^^^
                    //Grab this name here
                    int exclamationPointPosition = split[0].IndexOf("!");
                    string username = split[0].Substring(1, exclamationPointPosition - 1);

                    //Skip the first character, the first colon, then find the next colon
                    int secondColonPosition = line.IndexOf(':', 1); //the 1 here is what skips the first character
                    string message = line.Substring(secondColonPosition + 1); //Everything past the second colon
                    string channel = split[2].TrimStart('#');

                    await OnMessage.Invoke(this, new TwitchMessageEventArgs
                    {
                        Message = message,
                        User = _settings.Credentials.Username,
                        Sender = username
                    });
                }
            }

            _streamReader.Close();
            _streamWriter.Close();
            _streamReader.Dispose();
            _streamWriter.Dispose();

            _logger.LogInformation("Closed connection!");
        }
    }
}
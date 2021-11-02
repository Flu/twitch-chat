using System;
using System.IO;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using TwitchBot.Models;
using System.Threading.Tasks;

namespace TwitchBot.Services
{
    public class TwitchMessageEventArgs : EventArgs
    {
        public string Message { get; set; }
        public string User { get; set; }
    }

    public class TwitchService : ITwitchService
    {
        private readonly TwitchSettings _settings;

        public event TwitchChatEventHandler OnMessage;

        public TwitchService(TwitchSettings settings)
        {
            _settings = settings;
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return sslPolicyErrors == SslPolicyErrors.None;
        }

        public async Task StartListening(string channelName)
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
            var streamReader = new StreamReader(_settings.Ssl ? sslStream : tcp.GetStream());
            var streamWriter = new StreamWriter(_settings.Ssl ? sslStream : tcp.GetStream())
            {
                NewLine = "\r\n",
                AutoFlush = true
            };

            await streamWriter.WriteLineAsync($"PASS {_settings.Credentials.Secret}");
            await streamWriter.WriteLineAsync($"NICK {_settings.Credentials.Username}");
            await streamWriter.WriteLineAsync($"JOIN #{channelName}");

            while (true)
            {
                string line = await streamReader.ReadLineAsync();
                string[] split = line.Split(" ");

                //PING :tmi.twitch.tv
                //Respond with PONG :tmi.twitch.tv
                if (line.StartsWith("PING"))
                {
                    await streamWriter.WriteLineAsync($"PONG {split[1]}");
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

                    OnMessage.Invoke(this, new TwitchMessageEventArgs
                    {
                        Message = message,
                        User = username
                    });
                }
            }
        }
    }
}
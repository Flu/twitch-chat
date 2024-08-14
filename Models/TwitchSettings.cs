namespace TwitchBot.Models
{
    public class TwitchSettings
    {
        public Credentials Credentials { get; set; }
        public bool Ssl { get; set; }
        public string Url { get; set; }
        public int Port => Ssl ? 6697 : 6667;
        public string[] Channels { get; set; }

        public override string ToString() {
            return $"{Credentials} += {Url}:{Port}";
        }
    }
}

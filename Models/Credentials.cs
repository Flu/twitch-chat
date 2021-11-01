namespace TwitchBot.Models
{
    public class Credentials {
        public string Username { get; set; }
        public string Secret { get; set; }
        
        public override string ToString()
        {
            return $"{Username},{Secret}";
        }
    }
}
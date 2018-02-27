namespace EmojiButler.Models
{
    public class Configuration
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
        public ulong IssueChannel { get; set; }
        public string DblAuth { get; set; }
        public ulong BotId { get; set; }
    }
}
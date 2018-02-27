using Newtonsoft.Json;

namespace EmojiButler.Models
{
    public class Statistics
    {
        [JsonProperty("emoji")]
        public int Emoji { get; set; }

        [JsonProperty("users")]
        public int Users { get; set; }

        [JsonProperty("faves")]
        public int Favorites { get; set; }

        [JsonProperty("pending_approvals")]
        public int PendingApprovals { get; set; }

        public Statistics() { }
    }
}
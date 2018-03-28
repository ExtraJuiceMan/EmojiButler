using DSharpPlus.Entities;

namespace EmojiButler
{
    public static class Reactions
    {
        public static readonly DiscordEmoji YES = DiscordEmoji.FromName(EmojiButler.Shard, ":white_check_mark:");
        public static readonly DiscordEmoji NO = DiscordEmoji.FromName(EmojiButler.Shard, ":x:");
        public static readonly DiscordEmoji OK = DiscordEmoji.FromName(EmojiButler.Shard, ":ok_hand:");
    }
}
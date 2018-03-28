using DSharpPlus.Entities;

namespace EmojiButler
{
    public static class Reactions
    {
        public static readonly DiscordEmoji YES = DiscordEmoji.FromName(EmojiButler.Client, ":white_check_mark:");
        public static readonly DiscordEmoji NO = DiscordEmoji.FromName(EmojiButler.Client, ":x:");
        public static readonly DiscordEmoji OK = DiscordEmoji.FromName(EmojiButler.Client, ":ok_hand:");
    }
}
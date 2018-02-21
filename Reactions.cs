using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmojiButler
{
    public static class Reactions
    {
        public static readonly DiscordEmoji YES = DiscordEmoji.FromName(EmojiButler.client, ":white_check_mark:");
        public static readonly DiscordEmoji NO = DiscordEmoji.FromName(EmojiButler.client, ":x:");
    }
}

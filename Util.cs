using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EmojiButler
{
    public static class Util
    {
        public static void CreateCommandField(DiscordEmbedBuilder embed, Command c)
        {
            string summary = c.Description ?? "No description provided.";
            string command = c.Name;
            string prefix = EmojiButler.Configuration.Prefix;
            string usage = GenerateUsage(c);

            StringBuilder alias = null;

            if (c.Aliases != null)
            {
                alias = new StringBuilder();

                foreach (string s in c.Aliases)
                    alias.Append($"{prefix}{s} ");
            }

            embed.AddField($"{prefix}{command}",
                $"\t{summary}\n\tUsage: ``{usage}``\n\t{(alias != null ? $"Aliases: ``{alias}``" : "")}");
        }

        public static string GenerateUsage(Command c)
        {
            StringBuilder result = new StringBuilder($"{EmojiButler.Configuration.Prefix}{c.Name} ");

            foreach (CommandArgument arg in c.Arguments)
            {
                result.Append($"<{arg.Name}");
                if (arg.IsOptional)
                    result.Append(" (Optional)");
                result.Append("> ");
            }
            return result.ToString().Trim();
        }

        public static IReadOnlyDictionary<string, string> GetUnicodeEmojis()
        {
            PropertyInfo p = typeof(DiscordEmoji).GetProperty("UnicodeEmojis", BindingFlags.NonPublic | BindingFlags.Static);
            return (IReadOnlyDictionary<string, string>)p.GetValue(null);
        }

        public static int GetGuildCount(DiscordClient c) => c.Guilds.Count;
    }
}
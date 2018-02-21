using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EmojiButler
{
    public static class Util
    {
        public static void CreateCommandField(DiscordEmbedBuilder embed, Command c)
        {
            Console.WriteLine(c.Parent);
            string summary = c.Description ?? "No description provided.";
            string command = c.Name;
            string prefix = EmojiButler.configuration.Prefix;
            string usage = GenerateUsage(c);

            StringBuilder alias = new StringBuilder("None");

            if (c.Aliases != null)
            {
                alias.Clear();

                foreach (string s in c.Aliases)
                    alias.Append($"{prefix}{s} ");
            }

            embed.AddField($"{prefix}{command}",
                $"\t{summary}\n\tUsage: ``{usage}``\n\tAliases: {alias}");
        }

        public static string GenerateUsage(Command c)
        {
            string result = $"{EmojiButler.configuration.Prefix}{c.Name} ";
            foreach (CommandArgument arg in c.Arguments)
            {
                result += $"<{arg.Name}";
                if (arg.IsOptional)
                    result += " (Optional)";
                result += "> ";
            }
            return result.Trim();
        }
    }
}

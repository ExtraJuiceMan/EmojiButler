using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EmojiButler.Commands
{
    public class GeneralCommands
    {
        [Command("help")]
        [Description("Displays a help page.")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Help(CommandContext c)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "EmojiButler Manual",
                Description = "The official EmojiButler manual. EmojiButler is a bot that grabs emoji for you from [DiscordEmoji](https://discordemoji.com). All commands involving the management of emojis require the user and bot to have the 'Manage Emojis' permission."
            };

            try
            {
                foreach (KeyValuePair<string, Command> cmd in EmojiButler.commands.RegisteredCommands)
                {
                    if (cmd.Value != null)
                        Util.CreateCommandField(embed, cmd.Value);
                }
            }
            catch(Exception e) { Console.WriteLine(e.ToString()); }

            await c.RespondAsync(embed: embed);
        }

        [Command("emojify")]
        [Description("Emojifies some text.")]
        [Cooldown(5, 15, CooldownBucketType.User)]
        public async Task Emojify(CommandContext c, [RemainingText] string content)
        {
            // wtf dsharp
            if (content == null)
                throw new ArgumentException("Not enough arguments");

            IEnumerable<string> emojis = Util.GetUnicodeEmojis()
                .Select(x => x.Key)
                .Where(x => x.StartsWith(':') && x.EndsWith(':') && !x.Contains("skin-tone") && !x.Contains("flag_"));

            List<string> split = content.Split(' ').ToList();

            Random r = new Random();
            int count = r.Next(split.Count);
            for (int i = 0; i < count; i++)
            {
                string addedEmojis = "";
                int addCount = r.Next(4);
                for (int x = 0; x < addCount; x++)
                    addedEmojis += emojis.ElementAt(r.Next(emojis.Count()));
                split.Insert(r.Next(split.Count), addedEmojis);
            }

            string result = String.Join(' ', split);
            if (result.Length >= 2000)
                result = result.Substring(0, 1989) + " (trimmed)";

            await c.RespondAsync(result);
        }
    }
}

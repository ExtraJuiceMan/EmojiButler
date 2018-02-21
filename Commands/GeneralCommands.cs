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
                Description = "The official EmojiButler manual. All commands involving the management of emojis require the user and bot to have the 'Manage Emojis' permission."
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

        [Command("reportissue")]
        [Description("Reports an issue to the bot dev.")]
        [Cooldown(1, 15, CooldownBucketType.User)]
        public async Task ReportIssue(CommandContext c, string details)
        {
            DiscordChannel channel = await c.Client.GetChannelAsync(EmojiButler.configuration.IssueChannel);
            await channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = $"{c.Guild.Name} - {c.Guild.Id}",
                Description = details,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = c.User.Username,
                    IconUrl = c.User.AvatarUrl
                }
            });
            await c.Message.CreateReactionAsync(DiscordEmoji.FromName(c.Client, ":ok_hand:"));
        }
    }
}

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using EmojiButler.Models;
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
        [Command("reportissue")]
        [Description("Reports an issue to the bot dev.")]
        [Cooldown(1, 30, CooldownBucketType.User)]
        public async Task ReportIssue(CommandContext c, [RemainingText, Description("Issue to report")] string issue)
        {
            DiscordChannel issueChannel = await c.Client.GetChannelAsync(EmojiButler.configuration.IssueChannel);

            await issueChannel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = c.Guild != null ? $"{c.Guild.Name}" : "Direct Message",
                Description = "From Channel: " + c.Channel.Id + "\n" + issue
            }.WithAuthor(c.User.Username, null, c.User.AvatarUrl));

            await c.Message.CreateReactionAsync(Reactions.OK);
        }

        [Command("help")]
        [Description("Displays a help page.")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Help(CommandContext c)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "EmojiButler Manual",
                Description = "The official EmojiButler manual. EmojiButler is a bot that grabs emoji for you from [DiscordEmoji](https://discordemoji.com). All commands involving the management of emojis require the user and bot to have the 'Manage Emojis' permission.\n\nIf you like my bot, vote for it on [DBL](https://discordbots.org/bot/415637632660537355)! "
            }.AddField("\u200B", "**Commands**");

            foreach (Command cmd in EmojiButler.commands.RegisteredCommands.Select(x => x.Value))
                if (cmd != null && !cmd.IsHidden)
                    Util.CreateCommandField(embed, cmd);

            embed.AddField("\u200B", "**Other Stuff**\nThis bot is primarily an interface to add emojis to your server from [DiscordEmoji](https://discordemoji.com), you should check it out before using the bot." +
                "\n\n*The bot's logo is a modified version of the Jenkins (https://jenkins.io/) logo, and I am required by the license to link back to it.*");

            if (c.Guild != null)
            {
                await c.Member.SendMessageAsync(embed: embed);
                await c.Message.CreateReactionAsync(Reactions.OK);
            }
            else
                await c.RespondAsync(embed: embed);
        }

        [Command("destats")]
        [Description("Displays DiscordEmoji statistics.")]
        [Cooldown(5, 15, CooldownBucketType.User)]
        public async Task DeStats(CommandContext c)
        {
            Statistics s = EmojiButler.deClient.Statistics;
            await c.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Title = "DiscordEmoji Statistics"
            }
            .AddField("Emoji Count", s.Emoji.ToString())
            .AddField("Favorites Count", s.Favorites.ToString())
            .AddField("User Count", s.Users.ToString())
            .AddField("Pending Approvals", s.PendingApprovals.ToString())
            );
        }

        [Command("emojify")]
        [Description("Emojifies some text. :ok_hand:")]
        [Cooldown(5, 15, CooldownBucketType.User)]
        public async Task Emojify(CommandContext c, [RemainingText, Description("Content to emojify")] string content)
        {
            // wtf dsharp
            if (content == null)
                throw new ArgumentException("Not enough arguments");

            IEnumerable<string> emojis = Util.GetUnicodeEmojis()
                .Select(x => x.Key)
                .Where(x => x.StartsWith(':') && x.EndsWith(':') && !x.Contains("skin-tone") && !x.Contains("flag_"));

            List<string> split = content.Split(' ').ToList();

            Random r = new Random();
            int count = r.Next(1, split.Count);
            for (int i = 0; i < count; i++)
            {
                string addedEmojis = "";
                int addCount = r.Next(0, 4);

                if (content.Length < 100)
                    addCount += 1;

                for (int x = 0; x < addCount; x++)
                    addedEmojis += emojis.ElementAt(r.Next(emojis.Count()));

                split.Insert(r.Next(split.Count), addedEmojis);
            }

            string result = String.Join(' ', split);
            if (result.Length >= 2000)
                result = result.Substring(0, 1989) + " (trimmed)";

            await c.RespondAsync(result);
        }

        [Command("hi"), Description("If I'm alive, I'll wave. :wave:"), Cooldown(5, 15, CooldownBucketType.User)]
        public async Task Hi(CommandContext c) => await c.RespondAsync(":wave:");

        [Command("source"), Description("Gives you my sauce code. :spaghetti:"), Cooldown(5, 15, CooldownBucketType.User)]
        public async Task Source(CommandContext c) => await c.RespondAsync("https://github.com/ExtraConcentratedJuice/EmojiButler");

        [Command("info"), Description("Gives you some information about myself. :page_facing_up:"), Cooldown(5, 15, CooldownBucketType.User)]
        public async Task Info(CommandContext c) =>
            await c.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Information",
                Description = "Some information about EmojiButler.",
                ThumbnailUrl = c.Client.CurrentUser.AvatarUrl
            }
            .AddField("Library", "DSharpPlus 3.2.3")
            .AddField("Creator", "ExtraConcentratedJuice")
            .AddField("Guild Count", Util.GetGuildCount(c.Client).ToString()));

        [Command("server"), Description("Displays an invite to the EmojiButler server."), Cooldown(5, 15, CooldownBucketType.User)]
        public async Task Server(CommandContext c) => await c.RespondAsync("https://discord.gg/Ushqydb");

        [Command("invite"), Description("Displays a link to invite me to your server."), Cooldown(5, 15, CooldownBucketType.User)]
        public async Task Invite(CommandContext c) => await c.RespondAsync("https://discordapp.com/oauth2/authorize?client_id=415637632660537355&scope=bot&permissions=1073794112");
    }
}

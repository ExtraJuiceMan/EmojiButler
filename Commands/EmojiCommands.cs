using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using EmojiButler.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EmojiButler.Commands
{
    public class EmojiCommands
    {
        [Command("tryaddall")]
        [Description("Adds an emoji to your server.")]
        [Cooldown(4, 10, CooldownBucketType.Guild)]
        [RequirePermissions(DSharpPlus.Permissions.ManageEmojis)]
        public async Task TryAddAll(CommandContext c)
        {
            try
            {
                foreach (Emoji e in EmojiButler.deClient.Emoji)
                {
                    Console.WriteLine(e.Title);
                    Console.WriteLine(e.GetImageUrl());

                    using (Stream s = await e.GetImage())
                        await c.Guild.CreateEmojiAsync(e.Title, s);
                }
            }
            catch (Exception ex)
            {
                if (ex is RatelimitTooHighException exr)
                {
                    await c.RespondAsync($"I couldn't process the request due to the extreme ratelimits Discord has placed on emoji management. Try again in {(int)exr.RemainingTime.TotalMinutes} minute(s).");
                    return;
                }
            }
        }

        [Command("addemoji")]
        [Description("Adds an emoji to your server.")]
        [Cooldown(4, 10, CooldownBucketType.Guild)]
        [RequirePermissions(DSharpPlus.Permissions.ManageEmojis)]
        public async Task AddEmoji(CommandContext c, [Description("Name of the emoji to add")] string name,
            [Description("Optional name override")] string nameOverride = null)
        {
            InteractivityModule i = c.Client.GetInteractivityModule();
            Emoji emoji = EmojiButler.deClient.Emoji.FirstOrDefault(x => x.Title == name);

            string addedName = nameOverride ?? emoji.Title;

            if (emoji == null)
            {
                await c.RespondAsync("No emoji by that name was found on DiscordEmoji." +
                    "\nPlease select a valid emoji from the catalog at https://discordemoji.com" +
                    "\n\n(The emoji name is case sensitive. Don't include the colons in your command!)");
                return;
            }

            var allEmoji = await c.Guild.GetEmojisAsync();

            if (allEmoji.Where(x => !x.IsAnimated).Count() >= 50 && emoji.GetCategoryName() != "Animated")
            {
                await c.RespondAsync("It seems like you already have 50 emojis. That's the limit. Remove some before adding more.");
                return;
            }

            if (allEmoji.Where(x => x.IsAnimated).Count() >= 50 && emoji.GetCategoryName() == "Animated")
            {
                await c.RespondAsync("It seems like you already have 50 *animated* emojis. That's the limit. Remove some before adding more.");
                return;
            }



            DiscordGuildEmoji conflictingEmoji = (allEmoji.FirstOrDefault(x => x.Name == addedName));

            if (conflictingEmoji != null)
            {
                DiscordEmbedBuilder overwrite = new DiscordEmbedBuilder
                {
                    Title = "Overwrite Confirmation",
                    Description = $"An emoji that currently exists on this server that" +
                    " has a conflicting name with the emoji that you are attempting to add.\nOverwrite? This will delete the emoji. React in less than 30 seconds to confirm.",
                    ThumbnailUrl = $"https://cdn.discordapp.com/emojis/{conflictingEmoji.Id}.png"
                };
                overwrite.AddField("Name", conflictingEmoji.Name);
                DiscordMessage overwriteConfirm = await c.RespondAsync(embed: overwrite);

                await overwriteConfirm.CreateReactionAsync(Reactions.YES);
                await overwriteConfirm.CreateReactionAsync(Reactions.NO);

                ReactionContext overwriteReact = await i.WaitForReactionAsync(x => x == Reactions.YES || x == Reactions.NO, c.User,
                    TimeSpan.FromSeconds(30));

                await overwriteConfirm.DeleteAsync();
                if (overwriteReact != null)
                {
                    if (overwriteReact.Message == overwriteConfirm)
                    {
                        if (overwriteReact.Emoji == Reactions.NO)
                        {
                            await c.RespondAsync("Alright, I won't add the emoji.");
                            return;
                        }

                        try { await c.Guild.DeleteEmojiAsync(conflictingEmoji); }
                        catch (NotFoundException) { }
                    }
                    else
                    {
                        await overwriteConfirm.DeleteAsync();
                        await c.RespondAsync("You did not react to the original message. Aborting.");
                        return;
                    }
                }
                else
                {
                    await c.RespondAsync("No response was given. Aborting.");
                    return;
                }
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "Confirmation",
                Description = "Are you sure that you want to add this emoji to your server?" +
                    "\nReact in less than 30 seconds to confirm.",
                ThumbnailUrl = emoji.GetImageUrl()
            };
            embed.AddField("Name", emoji.Title);

            if (nameOverride != null)
                embed.AddField("Name Override", addedName);

            embed.AddField("Author", emoji.Author);

            DiscordMessage m = await c.RespondAsync(embed: embed);

            await m.CreateReactionAsync(Reactions.YES);
            await m.CreateReactionAsync(Reactions.NO);

            ReactionContext react = await i.WaitForReactionAsync(x => x == Reactions.YES || x == Reactions.NO, c.User,
                TimeSpan.FromSeconds(30));

            await m.DeleteAsync();
            if (react != null)
            {
                if (react.Message == m)
                {
                    if (react.Emoji == Reactions.YES)
                    {
                        DiscordMessage resp = await c.RespondAsync("Adding emoji...");

                        try
                        {
                            using (Stream s = await emoji.GetImage())
                                await c.Guild.CreateEmojiAsync(addedName, s, null, $"Added by {c.User.Username}");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            if (e is BadRequestException)
                            {
                                await resp.ModifyAsync("I failed to upload the requested emoji to Discord. It was probably too big.");
                                return;
                            }
                        }


                        await resp.ModifyAsync("", new DiscordEmbedBuilder
                        {
                            Title = "Success!",
                            Description = $"You've added :{addedName}: to your server.",
                            ThumbnailUrl = emoji.GetImageUrl()
                        });
                    }
                    else if (react.Emoji == Reactions.NO)
                        await c.RespondAsync("Okay then, I won't be adding that emoji.");
                }
                else
                {
                    await m.DeleteAsync();
                    await c.RespondAsync("You did not react to the original message. Aborting.");
                    return;
                }
            }
            else
                await c.RespondAsync("No response was given. Aborting.");
        }

        [Command("clearemoji")]
        [Description("Clears all existing emoji on the server.")]
        [Cooldown(4, 10, CooldownBucketType.Guild)]
        [RequirePermissions(DSharpPlus.Permissions.ManageEmojis)]
        public async Task ClearEmoji(CommandContext c)
        {
            InteractivityModule i = c.Client.GetInteractivityModule();

            IReadOnlyList<DiscordGuildEmoji> emojis = await c.Guild.GetEmojisAsync();

            if (!emojis.Any())
            {
                await c.RespondAsync("You have no emoji on this server to remove.");
                return;
            }

            DiscordMessage m = await c.RespondAsync("Are you sure that you want to clear all emoji from this server?");

            await m.CreateReactionAsync(Reactions.YES);
            await m.CreateReactionAsync(Reactions.NO);

            ReactionContext react = await i.WaitForReactionAsync(x => x == Reactions.YES || x == Reactions.NO, c.User,
               TimeSpan.FromSeconds(30));

            await m.DeleteAsync();
            if (react != null)
            {
                if (react.Message == m)
                {
                    DiscordMessage clear = await c.RespondAsync("Alright, I'm clearing all of the emojis on this server...");

                    foreach (DiscordGuildEmoji e in emojis)
                    {
                        try { await c.Guild.DeleteEmojiAsync(e); }
                        catch (NotFoundException) { }
                    }

                    await clear.ModifyAsync("I've cleared all of the emojis on this server.");
                }
                else
                {
                    await c.RespondAsync("You did not react to the original message. Aborting.");
                    return;
                }
            }
            else
                await c.RespondAsync("No response was given. Aborting.");
        }

        [Command("removeemoji")]
        [Description("Removes an existing emoji from the server.")]
        [Cooldown(5, 10, CooldownBucketType.Guild)]
        [RequirePermissions(DSharpPlus.Permissions.ManageEmojis)]
        public async Task RemoveEmoji(CommandContext c, [Description("Name of the emoji to remove")] string name)
        {
            IReadOnlyList<DiscordGuildEmoji> emojis = await c.Guild.GetEmojisAsync();

            if (!emojis.Any())
            {
                await c.RespondAsync("You have no emoji on this server to remove.");
                return;
            }

            DiscordGuildEmoji toRemove = emojis.FirstOrDefault(x => x.Name == name);

            if (toRemove == null)
            {
                await c.RespondAsync("I did not find any emoji by that name on this server to remove.");
                return;
            }

            await c.Guild.DeleteEmojiAsync(toRemove);

            await c.RespondAsync("Emoji successfully removed!");
        }
    }
}


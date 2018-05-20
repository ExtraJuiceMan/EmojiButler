using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Net.WebSocket;
using EmojiButler.Commands;
using EmojiButler.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmojiButler
{
    public static class EmojiButler
    {
        public static DiscordClient Client { get; private set; }
        public static DiscordEmojiClient EmojiClient { get; private set; }
        public static CommandsNextModule Commands { get; private set; }
        public static InteractivityModule Interactivity { get; private set; }
        public static Configuration Configuration { get; private set; }

        private static void Main(string[] args) => MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();

        private static async Task MainAsync(string[] args)
        {
            Configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("config.json"));
            EmojiClient = new DiscordEmojiClient();

            Client = new DiscordClient(new DiscordConfiguration
            {
                UseInternalLogHandler = true,
#if DEBUG
                LogLevel = LogLevel.Debug,
#endif
                Token = Configuration.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            });

            Commands = Client.UseCommandsNext(new CommandsNextConfiguration
            {
                EnableDefaultHelp = false,
                StringPrefix = Configuration.Prefix
            });

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Client.SetWebSocketClient<WebSocket4NetCoreClient>();

            Interactivity = Client.UseInteractivity(new InteractivityConfiguration());

            Commands.RegisterCommands<EmojiCommands>();
            Commands.RegisterCommands<GeneralCommands>();

            Commands.CommandErrored += async (CommandErrorEventArgs e) =>
            {
                // TODO: rework this terrible error handler
#if DEBUG
                client.DebugLogger.LogMessage(LogLevel.Debug, "Exception", e.Exception.ToString(), DateTime.Now);
#endif
                if (e.Exception is ArgumentException arg)
                {
                    // lol why even xd
                    if (arg.Message.StartsWith("Max message length"))
                    {
                        await e.Context.RespondAsync("I was able to generate a response, but it was a wayyy too long for Discord...");
                    }
                    else if (arg.Message.StartsWith("Could not convert"))
                    {
                        await e.Context.RespondAsync("You supplied an invalid argument.");
                    }
                    else
                    {
                        int argsPassed = e.Context.Message.Content.Split(' ').Length - 1;
                        await e.Context.RespondAsync($"{(argsPassed > e.Command.Arguments.Count ? "Too many" : "Not enough")} arguments were supplied to this command.\nUsage: ``{Util.GenerateUsage(e.Command)}``");
                    }
                }
                else if (e.Exception is ChecksFailedException ex)
                {
                    StringBuilder msg = new StringBuilder("Checks for this command have failed: ");

                    foreach (CheckBaseAttribute a in ex.FailedChecks)
                    {
                        if (a is RequirePermissionsAttribute)
                            msg.Append("\nMissing Permissions");
                        else if (a is CooldownAttribute cd)
                            msg.Append($"\nCooldown, {(int)cd.GetRemainingCooldown(e.Context).TotalSeconds}s left");
                    }

                    await e.Context.RespondAsync(msg.ToString());
                }
                else if (e.Exception is InvalidOperationException)
                {
                    await e.Context.RespondAsync("This command is not available for use in DMs.");
                }
                else if (e.Exception is CommandNotFoundException)
                {
                    if (e.Context.Guild == null)
                        await e.Context.RespondAsync("That's an invalid command.");
                }
                else if (e.Exception is UnauthorizedException)
                {
                    await e.Context.RespondAsync("I was not authorized to perform an action, please check that I have the proper permissions (Read/Send Messages, Manage Emojis, Embed Links). If this was a help command, make sure that you have DMs enabled.");
                }
                else
                {
                    await e.Context.RespondAsync($"An error has occurred. Please report this with ``{Configuration.Prefix}reportissue <details>``.");
                    Client.DebugLogger.LogMessage(LogLevel.Critical, "Error", e.Exception.ToString(), DateTime.Now);
                }
            };

            await Client.ConnectAsync();

            Client.Ready += async (ReadyEventArgs a) =>
            {
                await Client.UpdateStatusAsync(new DiscordGame($"{Configuration.Prefix}help | https://discordemoji.com"), UserStatus.DoNotDisturb);

                if (!String.IsNullOrWhiteSpace(Configuration.DblAuth))
                    using (CancellationTokenSource s = new CancellationTokenSource())
                        new Task(() => PostDBLAsync(), s.Token, TaskCreationOptions.LongRunning).Start();
            };

            await Task.Delay(-1);
        }

        private static async void PostDBLAsync()
        {
            HttpClient c = new HttpClient();

            c.DefaultRequestHeaders.Add("Authorization", Configuration.DblAuth);

            while (true)
            {
                HttpResponseMessage resp;

                using (StringContent s = new StringContent(JsonConvert.SerializeObject(new { server_count = Util.GetGuildCount(Client) }), Encoding.UTF8, "application/json"))
                    resp = await c.PostAsync($"https://discordbots.org/api/bots/{Configuration.BotId}/stats", s);

                if (resp.IsSuccessStatusCode)
                    Client.DebugLogger.LogMessage(LogLevel.Info, "DBLPost", "Post to DBL was successful.", DateTime.Now);
                else
                    Client.DebugLogger.LogMessage(LogLevel.Warning, "DBLPost", "Post to DBL was unsuccessful.", DateTime.Now);

                Thread.Sleep(TimeSpan.FromMinutes(5));
            }
        }
    }
}
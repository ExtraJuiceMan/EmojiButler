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
        public static DiscordClient client;
        public static DiscordEmojiClient deClient;
        public static CommandsNextModule commands;
        public static Configuration configuration;
        private static InteractivityModule interactivity;

        private static void Main(string[] args) => MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();

        private static async Task MainAsync(string[] args)
        {
            configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("config.json"));
            deClient = new DiscordEmojiClient();

            CancellationToken token;

            using (CancellationTokenSource s = new CancellationTokenSource())
                token = s.Token;

            new Task(() => deClient.RefreshEmojiAsync(), token, TaskCreationOptions.LongRunning).Start();

            client = new DiscordClient(new DiscordConfiguration
            {
                UseInternalLogHandler = true,
#if DEBUG
                LogLevel = LogLevel.Debug,
#endif
                Token = configuration.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            });

            commands = client.UseCommandsNext(new CommandsNextConfiguration
            {
                EnableDefaultHelp = false,
                StringPrefix = configuration.Prefix
            });

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                client.SetWebSocketClient<WebSocket4NetCoreClient>();

            interactivity = client.UseInteractivity(new InteractivityConfiguration());

            commands.RegisterCommands<EmojiCommands>();
            commands.RegisterCommands<GeneralCommands>();

            commands.CommandErrored += async (CommandErrorEventArgs e) =>
            {
                if (e.Exception is ArgumentException arg)
                {
                    // lol why even xd
                    if (arg.Message.StartsWith("Max message length"))
                        await e.Context.RespondAsync("I was able to generate a response, but it was a wayyy too long for Discord...");
                    else if (arg.Message.StartsWith("Could not convert"))
                        await e.Context.RespondAsync("You supplied an invalid argument.");
                    else
                    {
                        int argsPassed = e.Context.Message.Content.Split(' ').Length - 1;
                        await e.Context.RespondAsync($"{(argsPassed > e.Command.Arguments.Count ? "Too many" : "Not enough")} arguments were supplied to this command.\nUsage: ``{Util.GenerateUsage(e.Command)}``");
                    }
                }
                else if (e.Exception is ChecksFailedException ex)
                {
                    string msg = "Checks for this command have failed: ";
                    foreach (CheckBaseAttribute a in ex.FailedChecks)
                    {
                        if (a is RequirePermissionsAttribute)
                            msg += "\nMissing Permissions";
                        else if (a is CooldownAttribute cd)
                            msg += $"\nCooldown, {(int)cd.GetRemainingCooldown(e.Context).TotalSeconds}s left";
                    }
                    await e.Context.RespondAsync(msg);
                }
                else if (e.Exception is InvalidOperationException)
                    await e.Context.RespondAsync("This command is not available for use in DMs.");
                else if (e.Exception is CommandNotFoundException)
                {
                    if (e.Context.Guild == null)
                        await e.Context.RespondAsync("That's an invalid command.");
                }
                else if (e.Exception is UnauthorizedException uex)
                {
                    await e.Context.RespondAsync("I was not authorized to perform an action, please check that I have the proper permissions. If this was a help command, make sure that you have DMs enabled.");
                }
                else
                {
                    await e.Context.RespondAsync($"An error has occurred. Please report this with ``{configuration.Prefix}reportissue <details>``.");
                    client.DebugLogger.LogMessage(LogLevel.Critical, "Error", e.Exception.ToString(), DateTime.Now);
                }
            };

            await client.ConnectAsync();

            client.Ready += async (ReadyEventArgs a) =>
            {
                await client.UpdateStatusAsync(new DiscordGame($"{configuration.Prefix}help | https://discordemoji.com"), UserStatus.DoNotDisturb);

                if (!String.IsNullOrWhiteSpace(configuration.DblAuth))
                    new Task(() => PostDBLAsync(), token, TaskCreationOptions.LongRunning).Start();
            };

            await Task.Delay(-1);
        }

        private static async void PostDBLAsync()
        {
            HttpClient c = new HttpClient();
            c.DefaultRequestHeaders.Add("Authorization", configuration.DblAuth);

            while (true)
            {
                HttpResponseMessage resp;

                using (StringContent s = new StringContent(JsonConvert.SerializeObject(new { server_count = Util.GetGuildCount(client) }), Encoding.UTF8, "application/json"))
                    resp = await c.PostAsync($"https://discordbots.org/api/bots/{configuration.BotId}/stats", s);

                if (resp.IsSuccessStatusCode)
                    client.DebugLogger.LogMessage(LogLevel.Info, "DBLPost", "Post to DBL was successful.", DateTime.Now);
                else
                    client.DebugLogger.LogMessage(LogLevel.Warning, "DBLPost", "Post to DBL was unsuccessful.", DateTime.Now);

                Thread.Sleep(TimeSpan.FromMinutes(5));
            }
        }
    }
}
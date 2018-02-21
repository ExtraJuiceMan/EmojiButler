using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Net.WebSocket;
using EmojiButler.Commands;
using EmojiButler.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EmojiButler
{
    public class EmojiButler
    {
        public static DiscordClient client;
        public static DiscordEmojiClient deClient;
        static InteractivityModule interactivity;
        static CommandsNextModule commands;
        static Configuration configuration;

        static void Main(string[] args) => MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();

        static async Task MainAsync(string[] args)
        {
            configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("config.json"));
            deClient = new DiscordEmojiClient();

            CancellationToken token = new CancellationTokenSource().Token;
            new Task(() => deClient.RefreshEmoji(), token, TaskCreationOptions.LongRunning).Start();

            client = new DiscordClient(new DiscordConfiguration
            {
                #if DEBUG
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug,
                #endif
                Token = configuration.Token,
                TokenType = TokenType.Bot
            });

            commands = client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = configuration.Prefix
            });

            client.SetWebSocketClient<WebSocket4NetCoreClient>();
            interactivity = client.UseInteractivity(new InteractivityConfiguration());

            commands.RegisterCommands<EmojiCommands>();

            await client.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}

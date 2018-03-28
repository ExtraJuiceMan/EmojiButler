using DSharpPlus;
using EmojiButler.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmojiButler
{
    public class DiscordEmojiClient
    {
        public static readonly HttpClient client = new HttpClient();

        public List<Emoji> Emoji { get; private set; }
        public Dictionary<int, string> Categories { get; private set; }
        public Statistics Statistics { get; private set; }

        public const string BASE = "https://discordemoji.com/api";
        public const string BASE_ASSETS = "https://discordemoji.com/assets/emoji/";

        public DiscordEmojiClient()
        {
            Emoji = GetEmojisAsync().GetAwaiter().GetResult();
            Categories = GetCategoriesAsync().GetAwaiter().GetResult();
            Statistics = GetStatisticsAsync().GetAwaiter().GetResult();

            using (CancellationTokenSource s = new CancellationTokenSource())
                new Task(() => RefreshEmojiAsync(), s.Token, TaskCreationOptions.LongRunning).Start();
        }

        public async Task<List<Emoji>> GetEmojisAsync() =>
            await HttpGetAsync<List<Emoji>>(null);

        public async Task<Dictionary<int, string>> GetCategoriesAsync() =>
            await HttpGetAsync<Dictionary<int, string>>("categories");

        public async Task<Statistics> GetStatisticsAsync() =>
            await HttpGetAsync<Statistics>("stats");

        public async Task<List<Emoji>> SearchEmojisAsync(string query) =>
            await HttpGetAsync<List<Emoji>>("search", new Dictionary<string, string> { { "q", query } });

        public string GetCategoryName(int c) => Categories.GetValueOrDefault(c);

        private async Task<T> HttpGetAsync<T>(string requestType, Dictionary<string, string> parameters)
        {
            StringBuilder url = new StringBuilder(BASE);

            if (requestType != null)
                url.Append("?request=" + WebUtility.UrlEncode(requestType));

            if (parameters != null)
            {
                bool qMark = requestType == null;

                foreach (KeyValuePair<string, string> x in parameters)
                {
                    string start;

                    if (qMark)
                    {
                        start = "?";
                        qMark = false;
                    }
                    else
                        start = "&";

                    url.Append($"{start}{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}");
                }
            }

            using (HttpResponseMessage resp = await client.GetAsync(new Uri(url.ToString())))
            {
                string content = await resp.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(content);
            }
        }

        private async Task<T> HttpGetAsync<T>(string requestType) => await HttpGetAsync<T>(requestType, null);

        public async void RefreshEmojiAsync()
        {
            while (true)
            {
                Emoji = await GetEmojisAsync();
                Statistics = await GetStatisticsAsync();
                Categories = await GetCategoriesAsync();
                EmojiButler.ShardsClient.DebugLogger.LogMessage(LogLevel.Info, "EmojiButler", "Cached emoji list updated.", DateTime.Now);
                Thread.Sleep(TimeSpan.FromMinutes(5));
            }
        }
    }
}
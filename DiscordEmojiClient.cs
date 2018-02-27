using DSharpPlus;
using EmojiButler.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EmojiButler
{
    public class DiscordEmojiClient
    {
        public static readonly HttpClient client = new HttpClient();

        private List<Emoji> emoji;
        private Dictionary<int, string> categories;
        private Statistics statistics;

        public List<Emoji> Emoji { get => emoji; }
        public Dictionary<int, string> Categories { get => categories; }
        public Statistics Statistics { get => statistics; }

        public const string BASE = "https://discordemoji.com/api";
        public const string BASE_ASSETS = "https://discordemoji.com/assets/emoji/";

        public DiscordEmojiClient()
        {
            emoji = GetEmojisAsync().GetAwaiter().GetResult();
            categories = GetCategoriesAsync().GetAwaiter().GetResult();
            statistics = GetStatisticsAsync().GetAwaiter().GetResult();
        }

        public async Task<List<Emoji>> GetEmojisAsync()
        {
            using (HttpResponseMessage resp = await client.GetAsync(new Uri(BASE)))
            {
                string content = await resp.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Emoji>>(content);
            }
        }

        public async Task<Dictionary<int, string>> GetCategoriesAsync()
        {
            using (HttpResponseMessage resp = await client.GetAsync(new Uri(BASE + "?request=categories")))
            {
                string content = await resp.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Dictionary<int, string>>(content);
            }
        }

        public async Task<Statistics> GetStatisticsAsync()
        {
            using (HttpResponseMessage resp = await client.GetAsync(new Uri(BASE + "?request=stats")))
            {
                string content = await resp.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Statistics>(content);
            }
        }

        public async Task<List<Emoji>> SearchEmojisAsync(string query)
        {
            using (HttpResponseMessage resp = await client.GetAsync(new Uri(BASE + "?request=search&q=" + query)))
            {
                string content = await resp.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Emoji>>(content);
            }
        }

        public string GetCategoryName(int c) => categories.GetValueOrDefault(c);

        public async void RefreshEmojiAsync()
        {
            while (true)
            {
                emoji = await GetEmojisAsync();
                statistics = await GetStatisticsAsync();
                categories = await GetCategoriesAsync();
                EmojiButler.client.DebugLogger.LogMessage(LogLevel.Info, "EmojiButler", "Cached emoji list updated.", DateTime.Now);
                Thread.Sleep(TimeSpan.FromMinutes(5));
            }
        }
    }
}
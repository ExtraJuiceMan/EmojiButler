using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EmojiButler.Models
{
    public class Emoji
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("category")]
        public int Category { get; set; }

        [JsonProperty("faves")]
        public int Favorites { get; set; }

        [JsonProperty("submitted_by")]
        public string Author { get; set; }

        public Emoji() { }

        // Original stream didn't support seek, have to copy it into a memorystream
        public async Task<Stream> GetImage() =>
            new MemoryStream(await DiscordEmojiClient.client.GetByteArrayAsync(GetImageUrl()));

        public string GetCategoryName() => EmojiButler.deClient.Categories[Category];
        public string GetImageUrl() => $"{DiscordEmojiClient.BASE_ASSETS}{Slug}.{(GetCategoryName() == "Animated" ? "gif" : "png")}";
    }
}

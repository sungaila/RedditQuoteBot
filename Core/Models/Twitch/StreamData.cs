using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace RedditQuoteBot.Core.Models.Twitch
{
    [DebuggerDisplay("UserLogin = {UserLogin}, Title = {Title}, ViewerCount = {ViewerCount}")]
    internal class StreamData
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }

        [JsonPropertyName("user_login")]
        public string? UserLogin { get; set; }

        [JsonPropertyName("game_id")]
        public string? GameId { get; set; }

        [JsonPropertyName("game_name")]
        public string? GameName { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("viewer_count")]
        public int ViewerCount { get; set; }

        [JsonPropertyName("started_at")]
        public DateTime StartedAt { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("thumbnail_url")]
        public string? ThumbnailUrl { get; set; }

        [JsonPropertyName("is_mature")]
        public bool IsMature { get; set; }

        [JsonPropertyName("tag_ids")]
        public IList<string?>? TagIds { get; set; }
    }
}
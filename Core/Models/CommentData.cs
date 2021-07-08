using RedditQuoteBot.Core.Converters;
using System;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace RedditQuoteBot.Core.Models
{
    [DebuggerDisplay("Name = {Name}, Author = {Author}, Body = {Body}")]
    internal class CommentData
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("parent_id")]
        public string? ParentId { get; set; }

        [JsonPropertyName("link_id")]
        public string? LinkId { get; set; }

        [JsonPropertyName("archived")]
        public bool Archived { get; set; }

        [JsonPropertyName("locked")]
        public bool Locked { get; set; }

        [JsonPropertyName("quarantine")]
        public bool Quarantine { get; set; }

        [JsonPropertyName("is_submitter")]
        public bool IsSubmitter { get; set; }

        [JsonPropertyName("subreddit")]
        public string? Subreddit { get; set; }

        [JsonPropertyName("link_title")]
        public string? LinkTitle { get; set; }

        [JsonPropertyName("author")]
        public string? Author { get; set; }

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("created_utc")]
        [JsonConverter(typeof(MicrosecondEpochConverter))]
        public DateTime CreatedUtc { get; set; }

        [JsonIgnore]
        public DateTime CreatedLocal { get => CreatedUtc.ToLocalTime(); }

        [JsonPropertyName("body")]
        public string? Body { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;

namespace RedditQuoteBot.Core.Models
{
    [DebuggerDisplay("Kind = {Kind}")]
    internal class ChildData
    {
        [JsonPropertyName("kind")]
        public string? Kind { get; set; }

        [JsonPropertyName("data")]
        public CommentData? Data { get; set; }
    }
}

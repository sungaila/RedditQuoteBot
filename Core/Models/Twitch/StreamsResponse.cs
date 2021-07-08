using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RedditQuoteBot.Core.Models.Twitch
{
    internal class StreamsResponse
    {
        [JsonPropertyName("data")]
        public IList<StreamData>? Data { get; set; }
    }
}
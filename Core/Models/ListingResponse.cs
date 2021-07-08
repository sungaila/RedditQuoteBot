using System.Text.Json.Serialization;

namespace RedditQuoteBot.Core.Models
{
    internal class ListingResponse
    {
        [JsonPropertyName("kind")]
        public string? Kind { get; set; }

        [JsonPropertyName("data")]
        public ListingData? Data { get; set; }
    }
}
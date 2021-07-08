using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace RedditQuoteBot.Core.Models
{
    [DebuggerDisplay("Dist = {Dist}, Before = {Before}, After = {After}")]
    internal class ListingData
    {
        [JsonPropertyName("modhash")]
        public string? Modhash { get; set; }

        [JsonPropertyName("dist")]
        public int Dist { get; set; }

        [JsonPropertyName("after")]
        public string? After { get; set; }

        [JsonPropertyName("before")]
        public string? Before { get; set; }

        [JsonPropertyName("children")]
        public IList<ChildData>? Children { get; set; }
    }
}
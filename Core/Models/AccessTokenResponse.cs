using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;

namespace RedditQuoteBot.Core.Models
{
    [DebuggerDisplay("Token = {Token}, ExpiresAt = {ExpiresAt}")]
    internal class AccessTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? Token { get; set; }

        [JsonPropertyName("token_type")]
        public string? Type { get; set; }

        private long _expiresIn;

        [JsonPropertyName("expires_in")]
        public long ExpiresIn
        {
            get => _expiresIn;
            set
            {
                _expiresIn = value;
                ExpiresAt = DateTime.UtcNow.AddSeconds(_expiresIn);
            }
        }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }

        [JsonIgnore]
        public DateTime ExpiresAt { get; private set; } = DateTime.MaxValue;

        public override string? ToString() => Token;
    }
}

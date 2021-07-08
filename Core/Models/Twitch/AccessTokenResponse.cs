using System;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace RedditQuoteBot.Core.Models.Twitch
{
    [DebuggerDisplay("Token = {Token}, ExpiresAtLocal = {ExpiresAtLocal}")]
    internal class AccessTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? Token { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

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
                ExpiresAtUtc = DateTime.UtcNow.AddSeconds(_expiresIn);
            }
        }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }

        [JsonIgnore]
        public DateTime ExpiresAtUtc { get; private set; } = DateTime.MaxValue;

        [JsonIgnore]
        public DateTime ExpiresAtLocal { get => ExpiresAtUtc.ToLocalTime(); }

        public override string? ToString() => Token;
    }
}
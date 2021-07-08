using RedditQuoteBot.Core.Models.Twitch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RedditQuoteBot.Core
{
    /// <summary>
    /// A HTTP client to query information from Twitch.tv.
    /// </summary>
    public class TwitchClient
    {
        private readonly HttpClient _httpClient = new HttpClient();

        private readonly TimeSpan _minRatelimit = TimeSpan.FromSeconds(1);

        private AccessTokenResponse? AccessTokenResponse { get; set; }

        private DateTime LastRequest { get; set; } = DateTime.MinValue;

        /// <summary>
        /// The app's client ID given by Reddit.
        /// </summary>
        public string AppClientId { get; private set; }

        private string AppClientSecret { get; set; }

        /// <summary>
        /// The application name used for the User-Agent.
        /// </summary>
        public string ApplicationName { get; }

        /// <summary>
        /// The application version used for the User-Agent.
        /// </summary>
        public string ApplicationVersion { get; }

        /// <summary>
        /// The minimum period to wait between HTTP requests.
        /// </summary>
        public TimeSpan Ratelimit { get; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// The User-Agent used for HTTP requests.
        /// </summary>
        public string UserAgent { get; }

        /// <summary>
        /// Creates a new instance of the Twitch.tv HTTP client.
        /// </summary>
        /// <param name="appClientId">The app's client ID given by Twitch.tv.</param>
        /// <param name="appClientSecret">The app's client secret password given by Twitch.tv.</param>
        /// <param name="applicationName">The application version used for the User-Agent. Defaults to <c>GetType().Assembly.GetName().Name</c>.</param>
        /// <param name="applicationVersion">The application version used for the User-Agent. Defaults to <c>GetType().Assembly.GetName().Version.ToString()</c>.</param>
        /// <param name="ratelimit">The minimum period to wait between HTTP requests. Defaults to 10 seconds.</param>
        public TwitchClient(
            string appClientId,
            string appClientSecret,
            string? applicationName = null,
            string? applicationVersion = null,
            TimeSpan? ratelimit = null)
        {
            if (string.IsNullOrEmpty(appClientId))
                throw new ArgumentException("The app client id cannot be null or empty.", nameof(appClientId));

            if (string.IsNullOrEmpty(appClientSecret))
                throw new ArgumentException("The app client secret cannot be null or empty.", nameof(appClientSecret));

            AppClientId = appClientId;
            AppClientSecret = appClientSecret;

            ApplicationName = (!string.IsNullOrEmpty(applicationName) ? applicationName : GetType().Assembly.GetName().Name)!;
            ApplicationVersion = (!string.IsNullOrEmpty(applicationVersion) ? applicationVersion : GetType().Assembly.GetName().Version.ToString())!;

            Ratelimit = ratelimit ?? Ratelimit;

            UserAgent = $"script:{ApplicationName}:{ApplicationVersion}";
            Console.WriteLine($"User-Agent: \"{UserAgent}\"");

            _httpClient.DefaultRequestHeaders.Add("client-id", AppClientId);
            _httpClient.DefaultRequestHeaders.Add(
                "User-Agent",
                 Uri.EscapeDataString(UserAgent));
        }

        private async Task ThrottleRequestsAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine();
            var ratelimit = TimeSpan.FromTicks(Math.Max(Ratelimit.Ticks, _minRatelimit.Ticks));
            var availableAt = LastRequest.Add(ratelimit);

            if (DateTime.UtcNow < availableAt)
            {
                var availableIn = availableAt.Subtract(DateTime.UtcNow);

                Console.WriteLine($"Delay for {availableIn} (until {availableAt}).");
                await Task.Delay(availableIn, cancellationToken);
            }

            LastRequest = DateTime.UtcNow;
        }

        private async Task CheckAuthentication(CancellationToken cancellationToken)
        {
            if (AccessTokenResponse == null || string.IsNullOrEmpty(AccessTokenResponse.Token) || AccessTokenResponse.ExpiresAtUtc <= DateTime.UtcNow)
                await AuthenticateAsync(cancellationToken);
        }

        private async Task AuthenticateAsync(CancellationToken cancellationToken)
        {
            await ThrottleRequestsAsync(cancellationToken);

            Console.Write("Request OAuth2 access token (Twitch.tv) with HTTP basic authentication ... ");

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", AppClientId },
                { "client_secret", AppClientSecret },
                { "grant_type", "client_credentials" }
            });

            try
            {
                var response = await _httpClient.PostAsync("https://id.twitch.tv/oauth2/token", content, cancellationToken);
                string responseContent = await response.Content.ReadAsStringAsync();
                AccessTokenResponse = JsonSerializer.Deserialize<AccessTokenResponse>(responseContent);

                if (AccessTokenResponse?.Token == null)
                    throw new InvalidOperationException("Failed to receive access token.");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                       "Bearer",
                       AccessTokenResponse.Token);

                Console.WriteLine("succeeded.");
            }
            catch (TaskCanceledException)
            {
                // task cancellation can be ignored
            }
            catch (Exception ex)
            {
                Console.WriteLine("failed.");
                Console.WriteLine(ex);
                throw;
            }
        }

        /// <summary>
        /// Requests the current viewer count for a given user id.
        /// </summary>
        /// <param name="userId">ID of the user who is streaming.</param>
        /// <param name="cancellationToken">The token used to cancel the request.</param>
        /// <returns>Returns <see langword="null"/> if the user is not streaming.</returns>
        public async Task<int?> GetStreamViewerCount(string userId, CancellationToken cancellationToken)
        {
            await CheckAuthentication(cancellationToken);
            await ThrottleRequestsAsync(cancellationToken);

            Console.Write($"Request stream information (Twitch.tv) for user id {userId} ... ");
            StreamsResponse? result;

            try
            {
                var response = await _httpClient.GetAsync($"https://api.twitch.tv/helix/streams?user_login={userId}", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("failed.");
                    Console.WriteLine(response.ReasonPhrase);
                    return null;
                }

                var responseContent = await response.Content.ReadAsStreamAsync();
                result = await JsonSerializer.DeserializeAsync<StreamsResponse>(responseContent, null, cancellationToken);

                if (result == null)
                    throw new InvalidOperationException("Failed to receive listing response.");

                Console.WriteLine("succeeded.");
            }
            catch (TaskCanceledException)
            {
                // task cancellation can be ignored
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("failed.");
                Console.WriteLine(ex);
                throw;
            }

            if (result?.Data == null || !result.Data.Any())
                return null;

            return result.Data.Single().ViewerCount;
        }
    }
}
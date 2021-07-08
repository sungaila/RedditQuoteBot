using RedditQuoteBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RedditQuoteBot.Core
{
    /// <summary>
    /// A HTTP client to query comments on Reddit and post comments when certain conditions are met.
    /// </summary>
    public class RedditClient
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
        /// The Reddit user name used for posting comments.
        /// </summary>
        public string BotUserName { get; private set; }

        private string BotUserPassword { get; set; }

        /// <summary>
        /// The subreddits to query comments from.
        /// </summary>
        public IEnumerable<string> Subreddits { get; }

        /// <summary>
        /// The phrases the queried comments are scanned for.
        /// </summary>
        public IEnumerable<string> TriggerPhrases { get; }

        /// <summary>
        /// The quotes that might be posted as comments.
        /// </summary>
        public IEnumerable<string> Quotes { get; }

        /// <summary>
        /// The reddit user names that will be ignored and not replied to.
        /// </summary>
        public IEnumerable<string> IgnoredUserNames { get; }

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
        /// The maximum age of comments to consider for replies.
        /// </summary>
        public TimeSpan MaxCommentAge { get; } = TimeSpan.FromHours(8);

        /// <summary>
        /// The maximum replies within a single link.
        /// </summary>
        public int CommentLimit { get; } = 3;

        /// <summary>
        /// The User-Agent used for HTTP requests.
        /// </summary>
        public string UserAgent { get; }

        /// <summary>
        /// Creates a new instance of the Reddit HTTP client.
        /// </summary>
        /// <param name="appClientId">The app's client ID given by Reddit.</param>
        /// <param name="appClientSecret">The app's client secret password given by Reddit.</param>
        /// <param name="botUserName">The Reddit user name used for posting comments.</param>
        /// <param name="botUserPassword">The Reddit user password used for posting comments.</param>
        /// <param name="subreddits">The subreddits to query comments from.</param>
        /// <param name="triggerPhrases">The phrases the queried comments are scanned for.</param>
        /// <param name="quotes">The quotes that might be posted as comments.</param>
        /// <param name="ignoredUserNames">The reddit user names that will be ignored and not replied to.</param>
        /// <param name="applicationName">The application version used for the User-Agent. Defaults to <c>Assembly.GetEntryAssembly().GetName().Name</c>.</param>
        /// <param name="applicationVersion">The application version used for the User-Agent. Defaults to <c>Assembly.GetEntryAssembly().GetName().Version</c>.</param>
        /// <param name="ratelimit">The minimum period to wait between HTTP requests. Defaults to 10 seconds.</param>
        /// <param name="maxCommentAge">The maximum age of comments to consider for replies. Defaults to 8 hours.</param>
        /// <param name="commentLimit">The maximum replies within a single link.</param>
        public RedditClient(
            string appClientId,
            string appClientSecret,
            string botUserName,
            string botUserPassword,
            IEnumerable<string> subreddits,
            IEnumerable<string> triggerPhrases,
            IEnumerable<string> quotes,
            IEnumerable<string>? ignoredUserNames = null,
            string? applicationName = null,
            string? applicationVersion = null,
            TimeSpan? ratelimit = null,
            TimeSpan? maxCommentAge = null,
            int commentLimit = 3)
        {
            if (string.IsNullOrEmpty(appClientId))
                throw new ArgumentException("The app client id cannot be null or empty.", nameof(appClientId));

            if (string.IsNullOrEmpty(appClientSecret))
                throw new ArgumentException("The app client secret cannot be null or empty.", nameof(appClientSecret));

            if (string.IsNullOrEmpty(botUserName))
                throw new ArgumentException("The reddit bot name cannot be null or empty.", nameof(botUserName));

            if (string.IsNullOrEmpty(botUserPassword))
                throw new ArgumentException("The reddit bot password cannot be null or empty.", nameof(botUserPassword));

            if (subreddits == null)
                throw new ArgumentNullException(nameof(subreddits));

            if (!subreddits.Any())
                throw new ArgumentException("One subreddit must be defined at least.", nameof(subreddits));

            if (triggerPhrases == null)
                throw new ArgumentNullException(nameof(triggerPhrases));

            if (!triggerPhrases.Any())
                throw new ArgumentException("One trigger phrase must be defined at least.", nameof(triggerPhrases));

            if (quotes == null)
                throw new ArgumentNullException(nameof(quotes));

            if (!quotes.Any())
                throw new ArgumentException("One quote must be defined at least.", nameof(quotes));

            AppClientId = appClientId;
            AppClientSecret = appClientSecret;
            BotUserName = botUserName;
            BotUserPassword = botUserPassword;
            Subreddits = subreddits;
            TriggerPhrases = triggerPhrases;
            Quotes = quotes;
            IgnoredUserNames = ignoredUserNames ?? new List<string>();

            ApplicationName = (!string.IsNullOrEmpty(applicationName) ? applicationName : Assembly.GetEntryAssembly().GetName().Name)!;
            ApplicationVersion = (!string.IsNullOrEmpty(applicationVersion) ? applicationVersion : Assembly.GetEntryAssembly().GetName().Version.ToString())!;

            Ratelimit = ratelimit ?? Ratelimit;
            MaxCommentAge = maxCommentAge ?? MaxCommentAge;
            CommentLimit = Math.Max(commentLimit, 1);

            UserAgent = $"script:{ApplicationName}:{ApplicationVersion} (by /u/{BotUserName})";
            Console.WriteLine($"User-Agent: \"{UserAgent}\"");

            _httpClient.DefaultRequestHeaders.Add(
                "User-Agent",
                 Uri.EscapeDataString(UserAgent));

            _preparedTriggerPhrases = TriggerPhrases.Select(phrase => phrase.ToLowerInvariant());
        }

        /// <summary>
        /// Runs the cycle for querying comments and replying to matching candidates.
        /// This loops ends when <paramref name="cancellationToken"/> is used to cancel or an unhandled exception occurs.
        /// </summary>
        /// <param name="cancellationToken">The token used to cancel the loop.</param>
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Started execution of Reddit client.");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    foreach (var subreddit in Subreddits)
                    {
                        foreach (var comment in await GetCommentsAsync(subreddit, cancellationToken))
                        {
                            await PostReplyAsync(comment, GetQuote(out int quoteId), quoteId, cancellationToken);
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // task cancellation can be ignored
            }
            finally
            {
                Console.WriteLine("Stopped execution of Reddit client.");
            }
        }

        private async Task ThrottleRequestsAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine();
            var ratelimit = TimeSpan.FromTicks(Math.Max(Ratelimit.Ticks, _minRatelimit.Ticks));
            var availableAt = LastRequest.Add(ratelimit);

            if (DateTime.UtcNow < availableAt)
            {
                var availableIn = availableAt.Subtract(DateTime.UtcNow);

                Console.WriteLine($"Delay for {availableIn} ({availableAt}).");
                await Task.Delay(availableIn, cancellationToken);
            }

            LastRequest = DateTime.UtcNow;
        }

        private async Task CheckAuthentication(CancellationToken cancellationToken)
        {
            if (AccessTokenResponse == null || string.IsNullOrEmpty(AccessTokenResponse.Token) || AccessTokenResponse.ExpiresAt <= DateTime.UtcNow)
                await AuthenticateAsync(cancellationToken);
        }

        private async Task AuthenticateAsync(CancellationToken cancellationToken)
        {
            await ThrottleRequestsAsync(cancellationToken);

            Console.Write("Request OAuth2 access token with HTTP basic authentication ... ");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                AuthenticationSchemes.Basic.ToString(),
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{AppClientId}:{AppClientSecret}")));

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "username", BotUserName },
                { "password", BotUserPassword }
            });

            try
            {
                var response = await _httpClient.PostAsync("https://www.reddit.com/api/v1/access_token", content, cancellationToken);
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

        private readonly IEnumerable<string> _preparedTriggerPhrases;

        private async Task<IEnumerable<CommentData>> GetCommentsAsync(string subreddit, CancellationToken cancellationToken)
        {
            await CheckAuthentication(cancellationToken);
            await ThrottleRequestsAsync(cancellationToken);

            Console.Write($"Request comments for subreddit /r/{subreddit} ... ");
            ListingResponse? result = null;

            try
            {
                var response = await _httpClient.GetAsync($"https://oauth.reddit.com/r/{subreddit}/comments/?limit=100", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("failed.");
                    Console.WriteLine(response.ReasonPhrase);
                    return new List<CommentData>();
                }

                var responseContent = await response.Content.ReadAsStreamAsync();
                result = await JsonSerializer.DeserializeAsync<ListingResponse>(responseContent, null, cancellationToken);

                if (result == null)
                    throw new InvalidOperationException("Failed to receive listing response.");

                Console.WriteLine("succeeded.");
            }
            catch (TaskCanceledException)
            {
                // task cancellation can be ignored
                return new List<CommentData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("failed.");
                Console.WriteLine(ex);
                throw;
            }

            if (result.Data == null)
                return new List<CommentData>();

            var candidates = result.Data.Children.Select(child => child.Data!).ToList();
            var ownComments = candidates.Where(d => d!.Author == BotUserName).ToList();

#nullable disable
            var commentsToReply = candidates
                // ignore the junk
                .Where(c => c != null && !c.Archived && !c.Locked && !c.Quarantine && !string.IsNullOrEmpty(c.Body))
                // do not reply yourself
                .Where(c => c.Author != BotUserName)
                // ignore users from the given blacklist
                .Where(c => !IgnoredUserNames.Contains(c.Author))
                // skip comments being too old
                .Where(c => c.CreatedUtc >= DateTime.UtcNow.Subtract(MaxCommentAge))
                // do not reply multiple times
                .Where(c => ownComments.All(d => d.ParentId != c.Name))
                .Where(c => ReplyHistory.All(h => h.ParentId != c.Name))
                // do not exceed the maximum reply limit per parent link
                .Where(c => ReplyHistory.Count(h => h.LinkId == c.LinkId) < CommentLimit)
                // now check for any trigger phrase
                .Where(c => _preparedTriggerPhrases.Any(p => c.Body.ToLowerInvariant().Contains(p)))
                .ToList();
#nullable enable

            if (commentsToReply.Any())
                Console.WriteLine($"{commentsToReply.Count} candidates to reply to found.");
            else
                Console.WriteLine("Nothing.");

            return commentsToReply!;
        }

        private const int _randomRetryMultiplier = 100;
        private readonly Random _random = new Random();
        private readonly TimeSpan _sameQuoteTimeBuffer = TimeSpan.FromDays(1);

        private string GetQuote(out int quoteId)
        {
            int candidateId = 0;
            int maxRetryCount = Quotes.Count() * _randomRetryMultiplier;

            for (int i = 0; i < maxRetryCount; i++)
            {
                candidateId = _random.Next(Quotes.Count());

                if (ReplyHistory.All(h => h.QuoteId != candidateId || h.CreatedUtc < DateTime.UtcNow.Subtract(_sameQuoteTimeBuffer)))
                    break;
            }

            quoteId = candidateId;

            return Quotes.ElementAt(quoteId);
        }

        private string ApplyMacros(string quote, CommentData comment)
        {
            return quote
                .Replace("{author}", comment.Author)
                .Replace("{subreddit}", comment.Subreddit);
        }

        private async Task PostReplyAsync(CommentData comment, string quote, int quoteId, CancellationToken cancellationToken)
        {
            await CheckAuthentication(cancellationToken);
            await ThrottleRequestsAsync(cancellationToken);

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "parent", comment.Name! },
                { "text", ApplyMacros(quote, comment) }
            });

            Console.Write($"Post reply to comment \"{comment.Id}\" on subreddit \"/r/{comment.Subreddit}\" ... ");
            HttpResponseMessage? response;

            try
            {
                response = await _httpClient.PostAsync("https://oauth.reddit.com/api/comment", content);

                LogReply(comment, quoteId);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("succeeded.");
                    Console.WriteLine($"Quote: \"{quote}\"");
                }
                else
                {
                    Console.WriteLine("failed.");
                    Console.WriteLine(response.ReasonPhrase);
                }
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

        private const int _maxReplyHistoryCapacity = 1000;

        private Queue<HistoryEntry> ReplyHistory { get; } = new Queue<HistoryEntry>(_maxReplyHistoryCapacity);

        private void LogReply(CommentData comment, int quoteId)
        {
            if (comment.LinkId == null)
                throw new InvalidOperationException("The LinkId of a comment cannot be null.");

            ReplyHistory.Enqueue(new HistoryEntry(comment, quoteId));

            if (ReplyHistory.Count > _maxReplyHistoryCapacity)
                ReplyHistory.Dequeue();
        }

        private struct HistoryEntry
        {
            public HistoryEntry(CommentData comment, int quoteId)
            {
                LinkId = comment.LinkId!;
                ParentId = comment.ParentId!;
                QuoteId = quoteId;
                CreatedUtc = DateTime.UtcNow;
            }

            public string LinkId { get; set; }

            public string ParentId { get; set; }

            public int QuoteId { get; set; }

            public DateTime CreatedUtc { get; }

            public DateTime CreatedLocal { get => CreatedUtc.ToLocalTime(); }
        }
    }
}
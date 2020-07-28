using RedditQuoteBot.Core;
using System;
using System.Reflection;
using System.Threading;

namespace RedditQuoteBot.Console
{
    public class Program
    {
        public static void Main()
        {
            System.Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Name);
            System.Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Version);
            System.Console.WriteLine("Press any key to stop the execution.");
            System.Console.WriteLine();

            var client = new RedditClient(
                Settings.AppClientId,
                Settings.AppClientSecret,
                Settings.BotUserName,
                Settings.BotUserPassword,
                Settings.Subreddits,
                Settings.TriggerPhrases,
                Settings.Quotes,
                Settings.IgnoredUserNames,
                Settings.ApplicationName,
                Settings.ApplicationVersion,
                Settings.Ratelimit,
                Settings.MaxCommentAge,
                Settings.CommentLimit);

            var tokenSource = new CancellationTokenSource();
            var task = client.RunAsync(tokenSource.Token);

            System.Console.ReadKey(true);

            if (!task.IsCompleted)
            {
                System.Console.WriteLine("User cancelled execution.");
                tokenSource.Cancel();
                task.GetAwaiter().GetResult();
            }
        }
    }
}

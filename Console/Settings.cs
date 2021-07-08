using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RedditQuoteBot.Console
{
    public static class Settings
    {
        static Settings()
        {
            var iniParser = new FileIniDataParser();
            IniData iniData = iniParser.ReadFile("Config.ini");

            AppClientId = iniData["Credentials"]["AppClientId"];
            AppClientSecret = iniData["Credentials"]["AppClientSecret"];
            BotUserName = iniData["Credentials"]["BotUserName"];
            BotUserPassword = iniData["Credentials"]["BotUserPassword"];

            Ratelimit = TimeSpan.FromSeconds(int.Parse(iniData["Options"]["Ratelimit"]));
            MaxCommentAge = TimeSpan.FromSeconds(int.Parse(iniData["Options"]["MaxCommentAge"]));
            CommentLimit = int.Parse(iniData["Options"]["CommentLimit"]);
            RateComment = TimeSpan.FromSeconds(int.Parse(iniData["Options"]["RateComment"]));

            ApplicationName = iniData["UserAgent"]["ApplicationName"];
            ApplicationVersion = iniData["UserAgent"]["ApplicationVersion"];

            Subreddits = GetFileContent(File.ReadAllText("Subreddits.txt"));
            IgnoredUserNames = GetFileContent(File.ReadAllText("IgnoredUserNames.txt"));
            TriggerPhrases = GetFileContent(File.ReadAllText("TriggerPhrases.txt"));
            Quotes = GetFileContent(File.ReadAllText("Quotes.txt"));
        }

        private static IList<string> GetFileContent(string value)
        {
            if (value == null)
                return new List<string>();

            return value
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => !string.IsNullOrEmpty(line) && !line.StartsWith(';'))
                .ToList();
        }

        public static readonly string AppClientId;

        public static readonly string AppClientSecret;

        public static readonly string BotUserName;

        public static readonly string BotUserPassword;

        public static readonly TimeSpan Ratelimit;

        public static readonly TimeSpan MaxCommentAge;

        public static readonly int CommentLimit;

        public static readonly TimeSpan RateComment;

        public static readonly string ApplicationName;

        public static readonly string ApplicationVersion;

        public static readonly IList<string> Subreddits;

        public static readonly IList<string> IgnoredUserNames;

        public static readonly IList<string> TriggerPhrases;

        public static readonly IList<string> Quotes;

        public static class Twitch
        {
            static Twitch()
            {
                var iniParser = new FileIniDataParser();
                IniData iniData = iniParser.ReadFile("Config.ini");

                AppClientId = iniData["Twitch"]["AppClientId"];
                AppClientSecret = iniData["Twitch"]["AppClientSecret"];
            }

            public static readonly string AppClientId;

            public static readonly string AppClientSecret;
        }
    }
}
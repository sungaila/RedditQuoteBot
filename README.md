<img src="https://raw.githubusercontent.com/sungaila/RedditQuoteBot/master/Icon.png" align="left" width="64" height="64" alt="RedditQuoteBot Logo">

# RedditQuoteBot
A reddit bot scanning comments for trigger phrases to reply with predefined quotes.

The class library is built on top of **.NET Standard 2.0** and the console app on **.NET Core 3.1**.

Feel free to grab it from [NuGet.org](https://www.nuget.org/packages/RedditQuoteBot) or to fork it for your own needs!

<img src="https://raw.githubusercontent.com/sungaila/RedditQuoteBot/master/Content/Screenshot_1.0.0.png" width="390" alt="Screenshot from version 1.0.0">

## How do I get this bot running on my PC?
### 0. Get your app credentials from reddit
Open the [reddit app preferences](https://www.reddit.com/prefs/apps) and click on the *create app* button.
* **name** a name for your bot script
* **app type** choose **script**
* **description** you can leave this blank
* **about url** you can leave this blank
* **redirect url** type *http://127.0.0.1* (not needed for this use case)

Click on the *create app* button. The created app will be editable now.

Now enter the name of your reddit bot account under **developers** and press return.

Note down your **app client id** (the smaller random text on the top left) and **app client secret** (the longer random text right to the **secret** label).

Do not share this information and keep it to yourself!

### 1. Download the latest RedditQuoteBot.Console release
Open [the latest release](https://github.com/sungaila/RedditQuoteBot/releases/latest). If you are running Windows, you will most likely need the  **RedditQuoteBot.Console_x.x.x_standalone_win-x64.zip** archive.

Download the archive and extract it somewhere on your PC.

### 2. Fill in the credentials
Open the **Config.ini** file and fill in the following information:
* **AppClientId** the smaller random text from step 0
* **AppClientSecret** the larger random text from step 0
* **BotUserName** the reddit name of your bot account
* **BotUserPassword** the reddit password of your bot account

### 3. (Optional) Change settings of the bot
There are a few more settings within **Config.ini**.
You may want to change these, take note of the comments.

### 4. Choose the subreddits to observe
Open the **Subreddits.txt** file.
Enter each subreddit you wish your bot to observe as a new line.

### 5. (Optional) Choose the reddit users to ignore
Open the **IgnoredUserNames.txt** file.
The bot will not reply to users listed here.

### 6. Define the trigger phrases
Open the **TriggerPhrases.txt** file.
Write a new line for each phrase the bot should reply to.

Please note that the check is case insensitive (there is no difference between `we did it` and `WE did IT`).

Also note that the phrases are not checked as whole words. If your phrase is `use it`, then the bot will trigger on comments like `we should abuse it`.

### 7. Define the quotes
Open the **Quotes.txt** file.
Each line is a potential quote of the bot to use. Use the `{br}` macro for line breaks within the same quote.

There are a few more macros available like `{author}` or `{subreddit}`. Check the comments in **Quotes.txt** for more information.

Please note that the quotes are interpreted as [reddit markdown](https://www.reddit.com/wiki/markdown). So you might need to escape characters like # when used in your quotes. You can make use of this markdown for formatting as well.

### 8. Run the bot
Start **RedditQuoteBot.Console**. The console app will crash when the credentials are invalid (or other settings are invalid).

Otherwise enjoy your very own reddit bot! Just try to follow the [reddit Bottiquette](https://www.reddit.com/wiki/bottiquette) and stop when asked to.

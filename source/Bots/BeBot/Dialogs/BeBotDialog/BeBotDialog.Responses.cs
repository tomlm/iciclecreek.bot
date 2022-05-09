namespace BeBot.Dialogs
{
    public static class BeBotDialogText
    {
        public static readonly string[] Help = new string[]
        {
$@"### Welcome!
I'm **BeBot**, the hybrid worker bot.

Every Sunday I'll ask you what your plans are for the week.

You can answer with natural language like:
* I will be in city center Monday and Friday.

You can then ask questions about other people's schedule:
* Who will be in city center Monday?
* Where are lilich and sgellock today?

"
        };

        public static readonly string[] GreetingReplies = new string[]
        {
            " Hi!",
            " Hello!",
            " Hola!",
            " Greetings!"
        };

        public static readonly string[] CancelReplies = new string[]
        {
            " OK, starting over...",
            " Got it. Let's start fresh.",
            " Cancellation accomplished.",
            " Woops. Was I stuck? Let's reset."
        };

        public static readonly string[] GoodbyeReplies = new string[]
        {
            " See you later!",
            " Goodbye!",
            " Bye!",
            " See ya!",
            " Catch you later aligator...",
            " Hasta la pasta",
            " A river ditchy!"
        };

        public static readonly string[] HelpUserAlias = new string[]
        {
             " I need to know your alias...cause without it I'm lost.",
             " I need your alias, bots like me aren't so good with names.",
        };
        public static readonly string[] AskUserAlias = new string[]
        {
            " What is your alias?",
            " I need to know your alias. Can you please provide it?"
        };

        public static readonly string[] UserAliasChangedReplies = new string[]
        {
            " Your alias is now @${user.alias}",
            " Got it, @${user.alias}",
            " Cool, cool, cool, I know your alias is @${user.alias}.",
            " Roger dodger @${user.alias} is it.",
            " Hail @${user.alias}!"
        };

        public static readonly string[] ThanksReplies = new string[]
        {
            " No problem!", " I love being helpful.", " You are welcome.", " De nada", " It is my pleasure to help you."
        };
    }
}

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

        public static readonly string[] UnrecognizedResponse = new string[]
        {
            " No capiche...",
            " I'm sorry, I didn't understand that response.",
            " Hmmm...I don't get it.",
            " Do you need help? Because I didn't understand that.",
            " Oh oh, is your cat typing again?  That made no sense to me.",
            " Apprently I'm not smart enough to understand you.",
        };

        public static readonly string[] OpenQuestion = new string[]
        {
            " What can I do for you?",
            " What is your wish?",
            " Your wish is my command, so what is your wish?",
            " Ask me to do something, I dare ya! What do you wanna do?",
            " What do you wanna do now?",
        };

        public static readonly string[] GreetingReplies = new string[]
        {
            " Hi!",
            " Hello!",
            " What's up?!",
            " Hola!",
            " Greetings!"
        };

        public static readonly string[] ThanksReplies = new string[]
        {
            " No problem!", " I love being helpful.", " You are welcome.", " De nada", " It is my pleasure to help you."
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

        public static readonly string[] UserAlias_Help = new string[]
        {
             " I need to know your alias...cause without it I'm lost.",
             " I need your alias, bots like me aren't so good with names.",
        };
        public static readonly string[] UserAlias_Ask = new string[]
        {
            " What is your alias?",
            " I need to know your alias. Can you please provide it?"
        };

        public static readonly string[] UserAlias_Changed = new string[]
        {
            " Your alias is now @${user.alias}",
            " Got it, @${user.alias}",
            " Cool, cool, cool, I know your alias is @${user.alias}.",
            " Roger dodger @${user.alias} is it.",
            " Hail @${user.alias}!"
        };

        public static readonly string[] UserAlias_Bad = new string[]
        {
            "\n\nI didn't understand your response as an alias.  I'm looking for something like *tomlm* or *@tomlm*"
        };

        public static readonly string[] SetPlanWhen_Ask = new string[]
        {
            "\n\nWhen will you be in this week?",
            "\n\nWhen do you think you will be in this week?",
            "\n\nWhat days are we talking about?"
        };

        public static readonly string[] SetPlanWhen_Bad = new string[]
        {
            "\n\nI didn't understand your response as dates.  I'm looking for something like *monday and friday*",
            "\n\nHmmm, I didn't find a date.  A typical response would be like *monday and friday*"
        };

        public static readonly string[] SetPlanWhere_Bad = new string[]
        {
            "\n\nI didn't understand your response as a location.  I'm looking for something like 'work' or 'building 123'"
        };
        public static readonly string[] SetPlanWhere_Ask = new string[]
        {
            "\n\nWhere will you be? (home, work, ...)",
            "\n\nWhich location are you going to? (home, work, ...)",
            "\n\nGive me a hint...what location will be working at? (home, work, ...)"
        };
    }
}

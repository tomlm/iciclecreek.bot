namespace BeBot.Dialogs
{
    public partial class BeBotDialog
    {
        protected static readonly string[] Greetings = new string[]
        {
            " Hi!",
            " Hello!",
            " Hola!",
            " Greetings!"
        };

        protected static readonly string[] Goodbyes = new string[]
        {
            " See you later!",
            " Goodbye!",
            " Bye!",
            " See ya!",
            " Catch you later aligator...",
            " Hasta la pasta",
            " A river ditchy!"
        };

        protected static readonly string[] SetAliasResponse = new string[]
        {
            " Your alias is now @${user.alias}",
            " Got it, @${user.alias}",
            " Cool, cool, cool, alias is now @${user.alias}.",
            " Roger dodger @${user.alias}.",
            " Hail @${user.alias}!"
        };

        protected static readonly string[] ThanksResponse = new string[]
        {
            " No problem!", " I love being helpful.", " You are welcome.", " De nada", " It my pleasure to help you."
        };

        protected static readonly string[] ExplainAlias = new string[]
        {
             " I need to know your alias...cause without it I'm lost",
             " I need your alias, bots aren't so good with names.",
             " Can you give help a poor bot?"
        };
    }
}

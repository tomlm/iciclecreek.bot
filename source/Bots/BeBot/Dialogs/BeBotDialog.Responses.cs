namespace BeBot.Dialogs
{
    public partial class BeBotDialog
    {
        protected static readonly string[] HelpText = new string[]
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

        protected static readonly string[] AckAliasChanged = new string[]
        {
            " Your alias is now @${user.alias}",
            " Got it, @${user.alias}",
            " Cool, cool, cool, I know your alias is @${user.alias}.",
            " Roger dodger @${user.alias} is it.",
            " Hail @${user.alias}!"
        };

        protected static readonly string[] ThanksResponse = new string[]
        {
            " No problem!", " I love being helpful.", " You are welcome.", " De nada", " It is my pleasure to help you."
        };

        protected static readonly string[] ExplainAlias = new string[]
        {
             " I need to know your alias...cause without it I'm lost.",
             " I need your alias, bots like me aren't so good with names.",
        };
    }
}

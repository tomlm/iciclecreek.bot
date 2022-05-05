namespace BeBot.Dialogs
{
    public partial class BeBotDialog
    {
        protected static readonly string[] Greetings = new string[]
        {
            "Hi ${activity.from.name}!",
            "Hello ${activity.from.name}!",
            "Hola ${activity.from.name}!",
            "Greetings ${activity.from.name}!"
        };

        protected static readonly string[] Goodbyes = new string[]
        {
            "See you later!",
            "Goodbye!",
            "Bye!",
            "See ya!",
            "Catch you later aligator...",
            "Hasta la pasta",
            "A river ditchy!"
        };

        protected static readonly string[] SetAliasResponse = new string[]
        {
            "Your alias is now ${user.alias}",
            "Got it, ${user.alias}",
            "Cool, cool, cool, alias is now ${user.alias}.",
            "Roger dodger ${user.alias}.",
            "Hail ${user.alias}!"
        };

        protected static readonly string[] ThanksResponse = new string[]
        {
            "No problem!", "My pleasure", "You are welcome.", "De nada", "It's a pleasure to help you."
        };
    }
}

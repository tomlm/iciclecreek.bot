namespace BeBot.Dialogs
{
    public static class BeBotDialogText
    {
        public static string[] Welcome = new string[]
        {
@"### Welcome!
I'm **BeBot**, the hybrid worker bot.

Every Sunday I'll ask you what your plans are for the week.

"
        };

        public static readonly string[] Help = new string[]
        {
$@"

You can **set your schedule** by saying stuff like this:
* *I will be in city center Monday and Friday.*
* *My plan is to be at work on Mondays and Thursdays.*

You can ask **who** and **where** questions about your coworkers schedule:
* *Who will be in city center Monday?*
* *Where are lilich and sgellock today?*

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

        public static readonly string[] WhatNext = new string[]
        {
            " What can I do for you?",
            " What is your wish?",
            " Your wish is my command, so what is your wish?",
            " Ask me to do something, I dare ya! What do you wanna do?",
            " What do you wanna do now?",
        };

        public static readonly string[] UserAlias_Help = new string[]
        {
             " I need to know your alias to operate correctly.",
             " Bots like me work better with aliases, because we don't really understand names.",
             " I'm a dummy, I don't understand names, I need an alias to work.",
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

    }
}

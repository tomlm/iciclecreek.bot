namespace BeBot.Dialogs
{
    public static class SetPlanDialogText
    {
        public static readonly string[] HelpText = new string[]
        {
$@"

### Setting a plan 
To set the plan for the week you need to tell me when and where you are going to be.

Examples:
* *I'll be in City Center on Monday, Tuesday and Friday*
* *I'll be at work on Mondays and Tuesdays*
* *I will be in city center on Monday and in red west c on thursdays.*
"
        };

        public static readonly string[] When_Ask = new string[]
        {
            "\n\nWhen will you be in this week?",
            "\n\nWhen do you think you will be in this week?",
            "\n\nWhat days are we talking about?"
        };

        public static readonly string[] When_Bad = new string[]
        {
            "\n\nI didn't understand your response as dates.  I'm looking for something like *monday and friday*",
            "\n\nHmmm, I didn't find a date.  A typical response would be like *monday and friday*"
        };

        public static readonly string[] When_Changed = new string[]
        {
            "\n\nWhen: ${this.when}",
        };

        public static readonly string[] Where_Ask = new string[]
        {
            "\n\nWhere will you be? (home, work, ...)",
            "\n\nWhich location are you going to? (home, work, ...)",
            "\n\nGive me a hint...what location will be working at? (home, work, ...)"
        };

        public static readonly string[] Where_Bad = new string[]
        {
            "\n\nI didn't understand your response as a location.  I'm looking for something like 'work' or 'building 123'"
        };

        public static readonly string[] Where_Changed = new string[]
        {
            "\n\nWhere: ${where}",
        };

    }
}

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers
{

    [System.Diagnostics.DebuggerDisplay("[{StartOffset}-{EndOffset}]{Text}")]
    public class Token
    {
        public string Text { get; set; }
        public int StartOffset { get; set; }
        public int EndOffset { get; set; }
    }
}

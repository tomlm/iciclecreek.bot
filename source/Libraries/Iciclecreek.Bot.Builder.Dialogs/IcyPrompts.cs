using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Options;

namespace Iciclecreek.Bot.Builder.Dialogs
{
    public class IcyAttachmentPrompt : AttachmentPrompt
    {
        public IcyAttachmentPrompt()
            : base(nameof(AttachmentPrompt))
        { }
    }

    public class IcyChoicePrompt : ChoicePrompt
    {
        public IcyChoicePrompt()
            : base(nameof(ChoicePrompt))
        {

        }
    }

    public class IcyConfirmPrompt : ConfirmPrompt
    {
        public IcyConfirmPrompt()
            : base(nameof(ConfirmPrompt))
        {
        }
    }

    public class IcyDateTimePrompt : DateTimePrompt
    {
        public IcyDateTimePrompt()
            : base(nameof(DateTimePrompt))
        { }
    }

    public class IcyNumberPrompt<T> : NumberPrompt<T>
        where T : struct
    {
        public IcyNumberPrompt()
            : base(nameof(NumberPrompt<T>))
        {
        }
    }

    public class IcyTextPrompt : TextPrompt
    {
        public IcyTextPrompt()
            : base(nameof(TextPrompt))
        {
        }
    }
}

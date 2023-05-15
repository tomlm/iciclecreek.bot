using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Options;

namespace Iciclecreek.Bot.Builder.Dialogs
{
    /// <summary>
    /// Dependency injection for AttachmentPrompt
    /// </summary>
    /// <remarks>you can invoke like this: dc.Prompt&lt;AttachmentPrompt&gt;(...) </remarks>
    public class IcyAttachmentPrompt : AttachmentPrompt
    {
        public IcyAttachmentPrompt()
            : base(nameof(AttachmentPrompt))
        { }
    }

    /// <summary>
    /// Dependency injection for ChoicePrompt
    /// </summary>
    /// <remarks>you can invoke like this: dc.Prompt&lt;ChoicePrompt&gt;(...)</remarks>
    public class IcyChoicePrompt : ChoicePrompt
    {
        public IcyChoicePrompt()
            : base(nameof(ChoicePrompt))
        {

        }
    }

    /// <summary>
    /// Dependency injection for ConfirmPrompt
    /// </summary>
    /// <remarks>you can invoke like this: dc.Prompt&lt;ConfirmPrompt&gt;(...)</remarks>
    public class IcyConfirmPrompt : ConfirmPrompt
    {
        public IcyConfirmPrompt()
            : base(nameof(ConfirmPrompt))
        {
        }
    }

    /// <summary>
    /// Dependency injection for DateTimePrompt
    /// </summary>
    /// <remarks>you can invoke like this: dc.Prompt&lt;DateTimePrompt&gt;(...)</remarks>
    public class IcyDateTimePrompt : DateTimePrompt
    {
        public IcyDateTimePrompt()
            : base(nameof(DateTimePrompt))
        { }
    }

    /// <summary>
    /// Dependency injection for NumberPrompt
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>you can invoke like this: dc.Prompt&lt;NumberPrompt&gt;(...)</remarks>
    public class IcyNumberPrompt<T> : NumberPrompt<T>
        where T : struct
    {
        public IcyNumberPrompt()
            : base(nameof(NumberPrompt<T>))
        {
        }
    }

    /// <summary>
    /// Dependency injection for TextPrompt
    /// </summary>
    /// <remarks>you can invoke like this: dc.Prompt&lt;TextPrompt&gt;(...)</remarks>
    public class IcyTextPrompt : TextPrompt
    {
        public IcyTextPrompt()
            : base(nameof(TextPrompt))
        {
        }
    }
}

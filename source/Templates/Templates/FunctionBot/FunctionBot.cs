using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;

namespace Empty
{
    /// <summary>
    /// override virtual methods on base class to implement behavior for activities.
    /// </summary>
    public partial class FunctionBot : TeamsActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync("hello world", cancellationToken: cancellationToken);
        }
    }
}
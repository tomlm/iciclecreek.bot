using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Iciclecreek.Bot.Builder.Dialogs.Database.AdaptiveCards.Dialogs;

namespace Iciclecreek.Bot.Builder.Dialogs.AdaptiveCards
{
    /// <summary>
    /// Class which contains registration of components for Cards library.
    /// </summary>
    public class CardsBotComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // triggers
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<OnActionExecute>(OnActionExecute.Kind));

            // dialogs
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<AdaptiveCardDialog>(AdaptiveCardDialog.Kind));
        }
    }
}

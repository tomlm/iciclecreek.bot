using Lucy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy
{
    /// <summary>
    /// Class which contains registration of components for Icicilecreek custom recognizers
    /// </summary>
    public class LucyBotComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<LucyRecognizer>(LucyRecognizer.Kind));

            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<PatternConverter>>();
        }
    }
}

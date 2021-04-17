using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucene
{
    /// <summary>
    /// Class which contains registration of components for Icicilecreek custom recognizers
    /// </summary>
    public class LuceneBotComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<DynamicListRecognizer>(DynamicListRecognizer.Kind));
        }
    }
}

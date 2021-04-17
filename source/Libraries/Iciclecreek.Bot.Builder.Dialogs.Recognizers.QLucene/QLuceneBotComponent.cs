using Iciclecreek.Bot.Builder.Dialogs.Recognizers.QLucene;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers
{
    /// <summary>
    /// Class which contains registration of components for Icicilecreek custom recognizers
    /// </summary>
    public class QLuceneBotComponent :  BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<QLuceneRecognizer>(QLuceneRecognizer.Kind));
        }
    }
}

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Iciclecreek.Bot.Builder.Dialogs.Files
{
    /// <summary>
    /// Class which contains registration of components for CosmosDB.
    /// </summary>
    public class FilesBotComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<ReadTextFile>(ReadTextFile.Kind));
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<WriteTextFile>(WriteTextFile.Kind));
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<DeleteFile>(DeleteFile.Kind));
        }
    }
}

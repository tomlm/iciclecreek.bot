using Iciclecreek.Bot.Builder.Dialogs.Database.AzureStorage.Table;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Iciclecreek.Bot.Builder.Dialogs.Database.AzureStorage
{
    /// <summary>
    /// Class which contains registration of components for CosmosDB.
    /// </summary>
    public class AzureStorageBotComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // actions
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<CreateTable>(CreateTable.Kind));
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<DeleteTable>(DeleteTable.Kind));
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<RetrieveEntity>(RetrieveEntity.Kind));
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<EntityOperation>(EntityOperation.Kind));
        }
    }
}

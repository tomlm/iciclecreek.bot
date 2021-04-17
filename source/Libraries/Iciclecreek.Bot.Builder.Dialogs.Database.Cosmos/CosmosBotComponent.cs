using AdaptiveExpressions.Converters;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Iciclecreek.Bot.Builder.Dialogs.Database.Cosmos
{
    /// <summary>
    /// Class which contains registration of components for CosmosDB.
    /// </summary>
    public class CosmosBotComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // actions
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<CreateContainer>(CreateContainer.Kind));
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<CreateDatabase>(CreateDatabase.Kind));
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<CreateItem>(CreateItem.Kind));
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<DeleteContainer>(DeleteContainer.Kind));
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<DeleteDatabase>(DeleteDatabase.Kind));
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<DeleteItem>(DeleteItem.Kind));
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<GetItem>(GetItem.Kind));
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<QueryItems>(QueryItems.Kind));
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<ReplaceItem>(ReplaceItem.Kind));
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<UpsertItem>(UpsertItem.Kind));

            // converters
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<ObjectExpressionConverter<object>>>();
        }
    }
}

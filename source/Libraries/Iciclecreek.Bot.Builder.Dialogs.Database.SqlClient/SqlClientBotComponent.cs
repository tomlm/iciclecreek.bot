using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Iciclecreek.Bot.Builder.Dialogs.Database.SqlClient
{
    /// <summary>
    /// Class which contains registration of components for SqlClient.
    /// </summary>
    public class SqlClientBotComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Actions
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<ExecuteSql>(ExecuteSql.Kind));
        }
    }
}

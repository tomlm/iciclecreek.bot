using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Iciclecreek.Bot.Expressions.Humanizer
{
    /// <summary>
    /// Class which contains registration of components for CosmosDB.
    /// </summary>
    public class HumanizerComponentRegistration : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            HumanizerFunctions.Register();
        }
    }
}

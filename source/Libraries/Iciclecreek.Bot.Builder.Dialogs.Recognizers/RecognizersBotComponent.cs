using System.Collections.Generic;
using AdaptiveExpressions.Converters;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers
{
    /// <summary>
    /// Class which contains registration of components for Icicilecreek custom recognizers
    /// </summary>
    public class RecognizersBotComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<ThresholdRecognizer>(ThresholdRecognizer.Kind));
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<QuotedTextEntityRecognizer>(QuotedTextEntityRecognizer.Kind));
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<PersonNameEntityRecognizer>(PersonNameEntityRecognizer.Kind));
            services.AddSingleton<DeclarativeType>((sp) => new DeclarativeType<CsvEntityRecognizer>(CsvEntityRecognizer.Kind));

            // converters
            services.AddSingleton<JsonConverterFactory, JsonConverterFactory<ExpressionPropertyConverter<List<string>>>>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Bot.Builder;
using Newtonsoft.Json.Linq;

namespace Iciclecreek.Bot.Builder.Dialogs
{
    public static class RecognizerResultExtensions
    {
        /// <summary>
        /// GetEntities
        /// </summary>
        /// <typeparam name="T">type for object</typeparam>
        /// <param name="recognizerResult">recognizerResult</param>
        /// <param name="jsonPath">$..dates..values</param>
        /// <returns></returns>
        public static IEnumerable<T> GetEntities<T>(this RecognizerResult recognizerResult, string jsonPath)
        {
            foreach (var token in recognizerResult.Entities.SelectTokens(jsonPath)
                .Where(jt => !jt.Path.Contains("$instance")))
            {
                if (token is JArray)
                {
                    foreach (var entity in token)
                    {
                        yield return entity.ToObject<T>();
                    }
                }
                else
                {
                    yield return token.ToObject<T>();
                }
            }
        }

    }
}

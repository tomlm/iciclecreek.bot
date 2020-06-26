using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers
{
    /// <summary>
    /// Entity Recognizer which uses a CSV to define entities.
    /// </summary>
    /// <remarks>
    /// First row should be the column names.  The following are required column names:
    /// token : the token to match
    /// type : the name of the entity 
    /// Other column names will be mapped to the resulting entity object and can be full property paths, 
    /// Ie column name of "resolution.value" will set the resulting entity { "resolution": {"value":"{ValueThatColumnInMatchingRow}"} } }
    /// </remarks>
    /// <example>
    /// token,type,resolution.value
    /// x1,FooEntity,15
    /// x2,FooEntity,32
    /// y1,BarEntity,2.5
    /// The output will be recognizer.Entities["FooEntity"] = "x1" and resolution.value = 15;
    /// </example>
    public class CsvEntityRecognizer : EntityRecognizer
    {
        [JsonProperty("$kind")]
        public const string Kind = "Iciclecreek.PersonNameEntityRecognizer";

        private static JsonSerializer Serializer = new JsonSerializer() { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        // shared cache of url => entityMap for theUrl
        private static Dictionary<string, Dictionary<string, JObject>> entityMaps = new Dictionary<string, Dictionary<string, JObject>>();

        public HttpClient httpClient;

        public CsvEntityRecognizer(HttpClient httpClient = null) => this.httpClient = httpClient ?? new HttpClient();

        /// <summary>
        /// Gets or sets the entity types to look for entities in
        /// </summary>
        /// <remarks>
        /// The default is to look in "text" entity.
        /// </remarks>
        [JsonProperty("entityTypes")]
        public ExpressionProperty<List<string>> EntityTypes { get; set; } = new List<string>() { "text" };

        /// <summary>
        /// Gets or sets the url for csv with values
        /// </summary>
        /// <value>url to resource with CSV. First row defines the property names, first column should be text to look for.</value>
        [JsonProperty("url")]
        public StringExpression Url { get; set; }

        public override Task<IEnumerable<Entity>> RecognizeEntitiesAsync(DialogContext dialogContext, IEnumerable<Entity> entities, CancellationToken cancellationToken = default) => base.RecognizeEntitiesAsync(dialogContext, entities, cancellationToken);

        public override Task<IEnumerable<Entity>> RecognizeEntitiesAsync(DialogContext dialogContext, Activity activity, IEnumerable<Entity> entities, CancellationToken cancellationToken = default) => base.RecognizeEntitiesAsync(dialogContext, activity, entities, cancellationToken);

        public override async Task<IEnumerable<Entity>> RecognizeEntitiesAsync(DialogContext dialogContext, string text, string locale, IEnumerable<Entity> entities, CancellationToken cancellationToken = default)
        {
            List<Entity> newEntities = new List<Entity>();
            var em = await GetEntityMap(dialogContext);

            if (entities != null)
            {
                var entityTypes = EntityTypes.GetValue(dialogContext);
                foreach (JObject entity in entities.Where(e => entityTypes.Contains(e.Type)).Select(e => JObject.FromObject(e, Serializer)))
                {
                    text = ObjectPath.GetPathValue<string>(entity, "text");
                    if (!string.IsNullOrEmpty(text))
                    {
                        var tokens = TokenUtils.GetTokens(text);

                        foreach (var token in tokens)
                        {
                            if (em.TryGetValue(token.Text, out JObject result))
                            {
                                dynamic newEntity = result.DeepClone();
                                newEntity.text = token.Text;
                                newEntity.start = token.StartOffset;
                                newEntity.end = token.EndOffset;
                                newEntity.source = entity;
                                newEntities.Add(((JObject)newEntity).ToObject<Entity>());
                            }
                        }
                    }
                }
            }

            return newEntities;
        }

        protected virtual async Task<Dictionary<string, JObject>> GetEntityMap(DialogContext dc)
        {
            var url = this.Url?.GetValue(dc);
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(this.Url));
            }

            if (!entityMaps.TryGetValue(url, out var entityMap))
            {
                IEnumerable<string> lines = Array.Empty<string>();

                if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    Uri uri = new Uri(url);
                    if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                    {
                        var response = await httpClient.GetAsync(url);
                        var text = await response.Content.ReadAsStringAsync();
                        using (var reader = new System.IO.StringReader(text))
                        {
                            var lineList = new List<string>();
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                lineList.Add(line);
                            }
                            lines = lineList;
                        }
                    }
                    else
                    {
                        throw new Exception("Bad Uri");
                    }
                }
                else
                {
                    var path = url
                        .Replace('/', Path.DirectorySeparatorChar)
                        .Replace('\\', Path.DirectorySeparatorChar);
                    lines = File.ReadAllLines(path);
                }

                var firstLine = lines.First();
                var columns = firstLine.Split(',').Select(t => t.Trim()).ToArray();

                entityMap = new Dictionary<string, JObject>();
                foreach (var line in lines.Skip(1))
                {
                    var values = line.Split(',');

                    dynamic payload = new JObject();
                    string token = null;
                    for (int col = 0; col < columns.Length; col++)
                    {
                        string colName = columns[col];
                        if (colName == "token")
                        {
                            token = values[col].Trim();
                        }
                        else
                        {
                            if (int.TryParse(values[col], out int intValue))
                            {
                                ObjectPath.SetPathValue(payload, colName, intValue);
                            }
                            else if (float.TryParse(values[col], out float floatValue))
                            {
                                ObjectPath.SetPathValue(payload, colName, floatValue);
                            }
                            else
                            {
                                ObjectPath.SetPathValue(payload, colName, values[col].Trim());
                            }
                        }
                    }

                    entityMap[token] = payload;
                }

                entityMaps[url] = entityMap;
            }

            return entityMap;
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
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

    public class PersonNameEntityRecognizer : EntityRecognizer
    {
        [JsonProperty("$kind")]
        public const string Kind = "Iciclecreek.PersonNameEntityRecognizer";

        private static JsonSerializer Serializer = new JsonSerializer() { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        private static Lazy<HashSet<string>> defaultCommonWords = new Lazy<HashSet<string>>(() =>
        {
            var words = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            using (Stream resource = typeof(PersonNameEntityRecognizer)
                .GetTypeInfo()
                .Assembly
                .GetManifestResourceStream("Iciclecreek.Bot.Builder.Dialogs.Recognizers.Data.commonwords.txt"))
            {
                using (var reader = new StreamReader(resource))
                {
                    var line = reader.ReadLine();
                    while (line != null)
                    {
                        words.Add(line.Trim());
                        line = reader.ReadLine();
                    }
                }
            }

            return words;
        });

        private static Lazy<Dictionary<string, string>> defaultGivenNames = new Lazy<Dictionary<string, string>>(() =>
        {
            List<string> lines = new List<string>();
            using (Stream resource = typeof(PersonNameEntityRecognizer)
                .GetTypeInfo()
                .Assembly
                .GetManifestResourceStream("Iciclecreek.Bot.Builder.Dialogs.Recognizers.Data.givennames.csv"))
            {
                using (var reader = new StreamReader(resource))
                {
                    var line = reader.ReadLine();
                    while (line != null)
                    {
                        lines.Add(line);
                        line = reader.ReadLine();
                    }
                }
            }
            return ProcessGivenNames(lines);
        });

        private static Lazy<HashSet<string>> defaultSurnames = new Lazy<HashSet<string>>(() =>
        {
            var lines = new List<string>();
            using (Stream resource = typeof(PersonNameEntityRecognizer)
                .GetTypeInfo()
                .Assembly
                .GetManifestResourceStream("Iciclecreek.Bot.Builder.Dialogs.Recognizers.Data.surnames.csv"))
            {
                using (var reader = new StreamReader(resource))
                {
                    var line = reader.ReadLine();
                    while (line != null)
                    {
                        lines.Add(line);
                        line = reader.ReadLine();
                    }
                }
            }

            return new HashSet<string>(lines.Select(l => l.Trim()), StringComparer.InvariantCultureIgnoreCase);
        });

        private Dictionary<string, string> givenNames = null;
        private HashSet<string> surnames = null;
        private HttpClient httpClient;

        /// <summary>
        /// Gets or sets the name of the entity to look for person names in.
        /// </summary>
        /// <remarks>
        /// The default is to look in full text entity.
        /// </remarks>
        [JsonProperty("entityTypes")]
        public ExpressionProperty<List<string>> EntityTypes { get; set; } = new List<string>() { "text" };

        /// <summary>
        /// Gets or sets url source for csv file with NAME,[M|F|?] for given names.
        /// </summary>
        [JsonProperty("givenNamesUrl")]
        public StringExpression GivenNamesUrl { get; set; }

        /// <summary>
        /// Gets or sets alternate url for csv file with surnames.
        /// </summary>
        [JsonProperty("surnamesUrl")]
        public StringExpression SurnamesUrl { get; set; }

        public PersonNameEntityRecognizer(HttpClient httpClient = null) => this.httpClient = httpClient ?? new HttpClient();

        public override Task<IEnumerable<Entity>> RecognizeEntitiesAsync(DialogContext dialogContext, IEnumerable<Entity> entities, CancellationToken cancellationToken = default) => base.RecognizeEntitiesAsync(dialogContext, entities, cancellationToken);

        public override Task<IEnumerable<Entity>> RecognizeEntitiesAsync(DialogContext dialogContext, Activity activity, IEnumerable<Entity> entities, CancellationToken cancellationToken = default) => base.RecognizeEntitiesAsync(dialogContext, activity, entities, cancellationToken);

        public override async Task<IEnumerable<Entity>> RecognizeEntitiesAsync(DialogContext dialogContext, string text, string locale, IEnumerable<Entity> entities, CancellationToken cancellationToken = default)
        {
            List<Entity> newEntities = new List<Entity>();

            // Get data from incoming entities
            if (entities != null)
            {
                List<Entity> givenNames = new List<Entity>();
                List<Entity> surnames = new List<Entity>();
                var givenNameMap = await GetGivenNames(dialogContext);
                var surnameMap = await GetSurnames(dialogContext);
                var commonWords = defaultCommonWords.Value;
                var entityTypes = EntityTypes.GetValue(dialogContext.State);
                foreach (JObject entity in entities.Where(e => entityTypes.Contains(e.Type)).Select(e => JObject.FromObject(e, Serializer)))
                {
                    text = ObjectPath.GetPathValue<string>(entity, "text");
                    if (!string.IsNullOrEmpty(text))
                    {
                        var tokens = TokenUtils.GetTokens(text);
                        Token prevToken = null;

                        foreach (var token in tokens)
                        {
                            if (prevToken?.Text == "'" && token.Text == "s")
                            {
                                continue;
                            }

                            string possibleGivenName = token.Text;

                            // if it starts with Cap letter and has no other cap letters, then it is highly likely it's a name
                            bool isNamePattern = char.IsUpper(token.Text[0]) &&
                                token.Text.Where(c => char.IsUpper(c)).Count() == 1;

                            if (givenNameMap.TryGetValue(possibleGivenName, out string gender))
                            {
                                // if it is not a common word, or it is name pattern
                                if (isNamePattern || !commonWords.Contains(possibleGivenName))
                                {
                                    dynamic givenName = JObject.FromObject(entity, Serializer);
                                    givenName.type = "givenName";
                                    givenName.start = token.StartOffset;
                                    givenName.end = token.EndOffset;
                                    givenName.text = possibleGivenName;
                                    givenName.source = entity;
                                    givenName.resolution = new JObject();
                                    givenName.resolution.value = possibleGivenName;
                                    givenName.resolution.gender = gender;
                                    var newEntity = ((JObject)givenName).ToObject<Entity>();
                                    newEntities.Add(newEntity);
                                    givenNames.Add(newEntity);
                                }
                            }

                            // if any of the surname parts are a surname, then it's probably a hyphenated surname
                            string possibleSurname = token.Text;
                            var parts = possibleSurname.Split('-');
                            if (parts.Any(part => surnameMap.Contains(part)))
                            {
                                // if it is not a common word, or it is name pattern
                                if (isNamePattern || !commonWords.Contains(possibleGivenName))
                                {
                                    dynamic surname = JObject.FromObject(entity, Serializer);
                                    surname.type = "surname";
                                    surname.start = token.StartOffset;
                                    surname.end = token.EndOffset;
                                    surname.source = entity;
                                    surname.text = token.Text;
                                    surname.resolution = new JObject();
                                    surname.resolution.value = token.Text;
                                    var newEntity = ((JObject)surname).ToObject<Entity>();
                                    newEntities.Add(newEntity);
                                    surnames.Add(newEntity);
                                }
                            }

                            prevToken = token;
                        }
                    }
                }

                foreach (var givenName in givenNames)
                {
                    var surname = surnames.Where(sn => (int)sn.Properties["start"] == (int)givenName.Properties["end"] + 1).FirstOrDefault();
                    if (surname != null)
                    {
                        dynamic fullName = new JObject();
                        fullName.type = "fullName";
                        fullName.text = $"{givenName.Properties["text"]} {surname.Properties["text"]}";
                        fullName.start = (int)givenName.Properties["start"];
                        fullName.end = (int)surname.Properties["end"];
                        fullName.resolution = new JObject();
                        fullName.resolution.givenName = JObject.FromObject(givenName);
                        fullName.resolution.surname = JObject.FromObject(surname);
                        fullName.resolution.value = fullName.text;
                        newEntities.Add(((JObject)fullName).ToObject<Entity>());
                    }
                }

            }

            return newEntities;
        }


        private async Task<Dictionary<string, string>> GetGivenNames(DialogContext dc)
        {
            if (this.givenNames == null)
            {
                var givenNamesUrl = GivenNamesUrl?.GetValue(dc.State);
                if (!string.IsNullOrEmpty(givenNamesUrl))
                {
                    IEnumerable<string> lines = await getLinesFromUrl(givenNamesUrl);
                    this.givenNames = ProcessGivenNames(lines);
                }
                else
                {
                    this.givenNames = defaultGivenNames.Value;
                }
            }

            return this.givenNames;
        }

        private async Task<HashSet<string>> GetSurnames(DialogContext dc)
        {
            if (this.surnames == null)
            {
                var surnamesUrl = this.SurnamesUrl?.GetValue(dc.State);
                if (!string.IsNullOrEmpty(surnamesUrl))
                {
                    IEnumerable<string> lines = await getLinesFromUrl(surnamesUrl);
                    this.surnames = new HashSet<string>(lines.Select(l => l.Trim()), StringComparer.InvariantCultureIgnoreCase);
                }
                else
                {
                    this.surnames = defaultSurnames.Value;
                }

            }

            return this.surnames;
        }

        private async Task<IEnumerable<string>> getLinesFromUrl(string url)
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

            return lines;
        }

        private static Dictionary<string, string> ProcessGivenNames(IEnumerable<string> lines)
        {
            var gn = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var pair in lines.Select(l => l.Split(','))
                .Select(arr => new
                {
                    Name = ((string)arr[0]).Trim().Replace('+', '-'),
                    Gender = ((string)arr[1]).Trim()
                }))
            {
                if (gn.TryGetValue(pair.Name, out string gender))
                {
                    // if the same name occures twice with different genders
                    if (gn[pair.Name] != pair.Gender)
                    {
                        // then treat it as ambigious gender
                        gn[pair.Name] = "?";
                    }
                }
                else
                {
                    gn[pair.Name] = pair.Gender;
                }
            }
            return gn;
        }

    }
}

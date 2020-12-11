using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lucy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LucyPad2.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EntitiesController : ControllerBase
    {
        private JsonConverter patternModelConverter = new PatternModelConverter();

        private IDeserializer yamlDeserializer = new DeserializerBuilder()
                                                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                                    .Build();
        private ISerializer yamlToJsonSerializer = new SerializerBuilder()
                                                .JsonCompatible()
                                                .Build();

        private IMemoryCache _cache;

        public EntitiesController(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        [HttpPost]
        public EntitiesResponse Post([FromBody] EntitiesRequest request)
        {
            EntitiesResponse result = new EntitiesResponse();
            LucyEngine engine = null;
            if (!_cache.TryGetValue<LucyEngine>(request.yaml, out engine))
            {
                try
                {

                    var x = yamlDeserializer.Deserialize(new StringReader(request.yaml));
                    var json = yamlToJsonSerializer.Serialize(x);
                    var model = JsonConvert.DeserializeObject<LucyModel>(json, patternModelConverter);
                    engine = new LucyEngine(model, useAllBuiltIns: true);
                    _cache.Set(request.yaml, engine);
                }
                catch (Exception err)
                {
                    result.message = err.Message;
                    return result;
                }
            }

            if (engine.Warnings.Any())
            {
                result.message = String.Join("\n", engine.Warnings);
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            result.entities = engine.MatchEntities(request.text).ToArray();
            sw.Stop();
            result.elapsed = sw.ElapsedMilliseconds;
            return result;
        }
    }

    public class EntitiesRequest
    {
        public string yaml { get; set; }
        public string text { get; set; }
    }

    public class EntitiesResponse
    {
        public long elapsed { get; set; }
        public string message { get; set; }
        public LucyEntity[] entities { get; set; } = new LucyEntity[0];
    }

}

using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lucy
{
    public class PatternModelConverter : JsonConverter<PatternModel>
    {
        public override bool CanRead => true;

        public override PatternModel ReadJson(JsonReader reader, Type objectType, PatternModel existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                return new PatternModel((string)reader.Value);
            }
            else
            {
                return new PatternModel(JArray.Load(reader).ToObject<string[]>());
            }
        }

        public override void WriteJson(JsonWriter writer, PatternModel value, JsonSerializer serializer)
        {
            if (value.Count() > 1)
            {
                serializer.Serialize(writer, value.ToArray());
            }
            else
            {
                serializer.Serialize(writer, value.First());
            }
        }
    }
}

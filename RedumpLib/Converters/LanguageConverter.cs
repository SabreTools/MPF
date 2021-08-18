using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedumpLib.Data;

namespace RedumpLib.Converters
{
    /// <summary>
    /// Serialize Language enum values
    /// </summary>
    public class LanguageConverter : JsonConverter<Language?[]>
    {
        public override bool CanRead { get { return false; } }

        public override Language?[] ReadJson(JsonReader reader, Type objectType, Language?[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, Language?[] value, JsonSerializer serializer)
        {
            JArray array = new JArray();
            foreach (var val in value)
            {
                JToken t = JToken.FromObject(val.ShortName() ?? string.Empty);
                array.Add(t);
            }

            array.WriteTo(writer);
        }
    }
}
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedumpLib.Data;

namespace RedumpLib.Converters
{
    /// <summary>
    /// Serialize LanguageSelection enum values
    /// </summary>
    public class LanguageSelectionConverter : JsonConverter<LanguageSelection?[]>
    {
        public override bool CanRead { get { return false; } }

        public override LanguageSelection?[] ReadJson(JsonReader reader, Type objectType, LanguageSelection?[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, LanguageSelection?[] value, JsonSerializer serializer)
        {
            JArray array = new JArray();
            foreach (var val in value)
            {
                JToken t = JToken.FromObject(val.LongName() ?? string.Empty);
                array.Add(t);
            }

            array.WriteTo(writer);
        }
    }
}
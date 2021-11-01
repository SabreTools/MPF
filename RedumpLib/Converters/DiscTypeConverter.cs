using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedumpLib.Data;

namespace RedumpLib.Converters
{
    /// <summary>
    /// Serialize DiscType enum values
    /// </summary>
    public class DiscTypeConverter : JsonConverter<DiscType?>
    {
        public override bool CanRead { get { return false; } }

        public override DiscType? ReadJson(JsonReader reader, Type objectType, DiscType? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, DiscType? value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(value.LongName() ?? string.Empty);
            t.WriteTo(writer);
        }
    }
}
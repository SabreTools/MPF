using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedumpLib.Data;

namespace RedumpLib.Converters
{
    /// <summary>
    /// Serialize YesNo enum values
    /// </summary>
    public class YesNoConverter : JsonConverter<YesNo?>
    {
        public override bool CanRead { get { return false; } }

        public override YesNo? ReadJson(JsonReader reader, Type objectType, YesNo? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, YesNo? value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(value.LongName() ?? string.Empty);
            t.WriteTo(writer);
        }
    }
}
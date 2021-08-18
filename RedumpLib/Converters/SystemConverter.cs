using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedumpLib.Data;

namespace RedumpLib.Converters
{
    /// <summary>
    /// Serialize RedumpSystem enum values
    /// </summary>
    public class SystemConverter : JsonConverter<RedumpSystem?>
    {
        public override bool CanRead { get { return false; } }

        public override RedumpSystem? ReadJson(JsonReader reader, Type objectType, RedumpSystem? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, RedumpSystem? value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(value.ShortName() ?? string.Empty);
            t.WriteTo(writer);
        }
    }
}
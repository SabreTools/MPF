using System;
using MPF.Data;
using MPF.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedumpLib.Data;

namespace MPF.Converters
{
    /// <summary>
    /// Serialize KnownSystem enum values
    /// </summary>
    public class KnownSystemConverter : JsonConverter<KnownSystem?>
    {
        public override bool CanRead { get { return false; } }

        public override KnownSystem? ReadJson(JsonReader reader, Type objectType, KnownSystem? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, KnownSystem? value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(value.ToRedumpSystem().ShortName() ?? value.ShortName() ?? string.Empty);
            t.WriteTo(writer);
        }
    }
}
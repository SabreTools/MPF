using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedumpLib.Data;

namespace MPF.Core.Converters
{
    /// <summary>
    /// Serialize MediaType enum values
    /// </summary>
    public class MediaTypeConverter : JsonConverter<MediaType?>
    {
        public override bool CanRead { get { return false; } }

        public override MediaType? ReadJson(JsonReader reader, Type objectType, MediaType? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, MediaType? value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(value.ShortName() ?? string.Empty);
            t.WriteTo(writer);
        }
    }
}
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedumpLib.Data;

namespace RedumpLib.Converters
{
    /// <summary>
    /// Serialize DiscCategory enum values
    /// </summary>
    public class DiscCategoryConverter : JsonConverter<DiscCategory?[]>
    {
        public override bool CanRead { get { return false; } }

        public override DiscCategory?[] ReadJson(JsonReader reader, Type objectType, DiscCategory?[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, DiscCategory?[] value, JsonSerializer serializer)
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
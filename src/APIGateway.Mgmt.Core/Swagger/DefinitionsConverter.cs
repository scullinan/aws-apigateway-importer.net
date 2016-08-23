using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace APIGateway.Swagger
{
    public class DefinitionsConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            var definitions = (IDictionary<string, Schema>)value;
            if (definitions.Any())
            {
                foreach (var entry in definitions)
                {
                    writer.WritePropertyName(entry.Key);
                    serializer.Serialize(writer, entry.Value);
                }
            }

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}
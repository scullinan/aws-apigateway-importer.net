using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

// ReSharper disable once CheckNamespace
namespace APIGateway.Swagger
{
    public class ResponseConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jsonContract = (JsonObjectContract)serializer.ContractResolver.ResolveContract(value.GetType());

            writer.WriteStartObject();

            foreach (var jsonProp in jsonContract.Properties)
            {
                var propValue = jsonProp.ValueProvider.GetValue(value);
                if (propValue == null && serializer.NullValueHandling == NullValueHandling.Ignore)
                    continue;

                if (jsonProp.PropertyName == "headers")
                {

                    var headers = (IDictionary<string, Header>)propValue;
                    if (headers.Any())
                    {
                        writer.WritePropertyName(jsonProp.PropertyName);
                        writer.WriteStartObject();

                        foreach (var entry in headers)
                        {
                            writer.WritePropertyName(entry.Key);
                            serializer.Serialize(writer, entry.Value);
                        }

                        writer.WriteEndObject();
                    }
                }
                else
                {
                    writer.WritePropertyName(jsonProp.PropertyName);
                    serializer.Serialize(writer, propValue);

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
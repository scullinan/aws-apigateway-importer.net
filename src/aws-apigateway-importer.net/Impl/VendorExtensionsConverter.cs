using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace aws_apigateway_importer.net.Impl
{
    public class VendorExtensionsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.GetField("vendorExtensions") != null;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var contract = (JsonObjectContract) serializer.ContractResolver.ResolveContract(objectType);
            existingValue = existingValue ?? contract.DefaultCreator();
            var jObject = JObject.Load(reader);

            foreach (var property in contract.Properties)
            {
                if (property.PropertyName == "vendorExtensions")
                {
                    var extentions = jObject.Properties().Where(x => x.Name.StartsWith("x-"));
                    var target = extentions.ToDictionary<JProperty, string, object>(extention => extention.Name, extention => extention.Value);

                    property.ValueProvider.SetValue(existingValue, target);
                }
                else
                {
                    var jProperty = jObject.Property(property.PropertyName);
                    if (jProperty != null)
                    {
                        using (var subReader = jProperty.Value.CreateReader())
                        {
                            var propertyValue = serializer.Deserialize(subReader, property.PropertyType);
                            property.ValueProvider.SetValue(existingValue, propertyValue);
                        }
                    }
                }
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jsonContract = (JsonObjectContract)serializer.ContractResolver.ResolveContract(value.GetType());

            writer.WriteStartObject();

            foreach (var jsonProp in jsonContract.Properties)
            {
                var propValue = jsonProp.ValueProvider.GetValue(value);
                if (propValue == null && serializer.NullValueHandling == NullValueHandling.Ignore)
                    continue;

                if (jsonProp.PropertyName == "VendorExtensions")
                {
                    var vendorExtensions = (IDictionary<string, object>)propValue;
                    if (vendorExtensions.Any())
                    {
                        foreach (var entry in vendorExtensions)
                        {
                            writer.WritePropertyName(entry.Key);
                            serializer.Serialize(writer, entry.Value);
                        }
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
    }
}
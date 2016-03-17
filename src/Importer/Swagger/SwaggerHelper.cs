using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Importer.Swagger
{
    public static class SwaggerHelper
    {
        public static string GetProducesContentType(IEnumerable<string> apiProduces, IEnumerable<string> methodProduces)
        {
            if (methodProduces != null && methodProduces.Any())
            {
                if (methodProduces.Any(t => t.Equals(Constants.DefaultProducesContentType, StringComparison.OrdinalIgnoreCase)))
                {
                    return Constants.DefaultProducesContentType;
                }

                return methodProduces.FirstOrDefault();
            }

            if (apiProduces != null && apiProduces.Any())
            {
                if (apiProduces.Any(t => t.Equals(Constants.DefaultProducesContentType, StringComparison.OrdinalIgnoreCase)))
                {
                    return Constants.DefaultProducesContentType;
                }

                return apiProduces.FirstOrDefault();
            }

            return Constants.DefaultProducesContentType;
        }

        public static string GenerateSchema(Schema model, string modelName, IDictionary<string, Schema> definitions)
        {
            try
            {
                var settings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    DateFormatHandling = DateFormatHandling.IsoDateFormat
                };

                var modelSchema = JsonConvert.SerializeObject(model, settings);
                var models = JsonConvert.SerializeObject(definitions, settings);

                var schema = new SchemaTransformer().Flatten(modelSchema, models);

                return schema;
            }
            catch (IOException e)
            {
                throw new ArgumentException("Could not process model", e);
            }
        }
    }
}

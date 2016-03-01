using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace AWS.APIGateway.Impl
{
    public partial class ApiGatewaySdkSwaggerApiImporter
    {
        private string GetApiName(SwaggerDocument swagger, string fileName)
        {
            var title = swagger.Info.Title;
            return !string.IsNullOrEmpty(title) ? title : fileName;
        }

        private string GenerateSchema(Schema model, string modelName, IDictionary<string, Schema> definitions)
        {
            return GenerateSchemaString(model, modelName, definitions);
        }

        private string GenerateSchemaString(object model, string modelName, IDictionary<string, Schema> definitions)
        {
            try
            {
                var modelSchema = JsonConvert.SerializeObject(model);
                var models = JsonConvert.SerializeObject(definitions);

                // inline all references
                var schema = new SchemaTransformer().Flatten(modelSchema, models); //Todo

                Log.InfoFormat("Generated json-schema for model {0} : {1}", modelName, schema);

                return schema;
            }
            catch (IOException e)
            {
                throw new ArgumentException("Could not process model", e);
            }
        }

        private string GetProducesContentType(IEnumerable<string> apiProduces, IEnumerable<string> methodProduces)
        {

            if (methodProduces != null && methodProduces.Any())
            {
                if (methodProduces.Any(t => t.Equals(DEFAULT_PRODUCES_CONTENT_TYPE, StringComparison.OrdinalIgnoreCase)))
                {
                    return DEFAULT_PRODUCES_CONTENT_TYPE;
                }

                return methodProduces.FirstOrDefault();
            }

            if (apiProduces != null && apiProduces.Any())
            {
                if (apiProduces.Any(t => t.Equals(DEFAULT_PRODUCES_CONTENT_TYPE, StringComparison.OrdinalIgnoreCase)))
                {
                    return DEFAULT_PRODUCES_CONTENT_TYPE;
                }

                return apiProduces.FirstOrDefault();
            }

            return DEFAULT_PRODUCES_CONTENT_TYPE;
        }
    }
}

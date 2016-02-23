using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;

namespace AWS.APIGateway.Impl
{
    public partial class ApiGatewaySdkSwaggerApiImporter : ApiGatewaySdkApiImporter, ISwaggerApiImporter
    {
        ILog log = LogManager.GetLogger(typeof (ApiGatewaySwaggerApiFileImporter));

        private static string DEFAULT_PRODUCES_CONTENT_TYPE = "application/json";
        private static string EXTENSION_AUTH = "x-amazon-apigateway-auth";
        private static string EXTENSION_INTEGRATION = "x-amazon-apigateway-integration";
        protected SwaggerDocument Swagger;

        public async Task<string> CreateApi(SwaggerDocument swagger, string name)
        {
            this.Swagger = swagger;
            ProcessedModels.Clear();
            var response = await CreateApi(GetApiName(swagger, name), swagger.Info.Description);
            log.InfoFormat("Created API {0}", response.Id);

            try
            {
                var api = response.RestApi();

                var rootResource = await GetRootResource(api);
                await DeleteDefaultModels(api);
                await CreateModels(api, swagger.Definitions, swagger.Produces);
                await CreateResources(api, rootResource, swagger.BasePath, swagger.Produces, swagger.Paths, true);

            }
            catch (Exception ex)
            {
                log.Error("Error creating API, rolling back", ex);
            }

            return string.Empty;
        }

        public async Task UpdateApi(string apiId, SwaggerDocument swagger)
        {
            log.Info("UpdateApi");
            await Task.FromResult(0);
        }

        public async Task Deploy(string apiId, string deploymentStage)
        {
            log.Info("Deploy");
            await Task.FromResult(0);
        }

        public async Task DeleteApi(string apiId)
        {
            log.Info("DeleteApi");
            await Task.FromResult(0);
        }

        private string GetApiName(SwaggerDocument swagger, string fileName)
        {
            var title = swagger.Info.Title;
            return !string.IsNullOrEmpty(title) ? title : fileName;
        }
        
        private string GenerateSchema(Schema model, String modelName, IDictionary<string, Schema> definitions)
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
    
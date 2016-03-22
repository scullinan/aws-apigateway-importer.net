using System.IO;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Importer.Swagger.Impl
{
    public class SwaggerApiFileImporter : ISwaggerApiFileImporter
    {
        private readonly ISwaggerApiImporter importer;
        ILog log = LogManager.GetLogger(typeof(SwaggerApiFileImporter));

        public SwaggerApiFileImporter(ISwaggerApiImporter importer)
        {
            this.importer = importer;
        }

        public string ImportApi(string filePath)
        {
            log.InfoFormat("Attempting to create API from Swagger definition. Swagger file: {0}", filePath);

            var swagger = Import<SwaggerDocument>(filePath);
            return importer.CreateApi(swagger, Path.GetFileName(filePath));
        }

        public void UpdateApi(string apiId, string filePath)
        {
            log.InfoFormat("Attempting to update API from Swagger definition. API identifier: {0} Swagger file: {1}", apiId, filePath);

            var swagger = Import<SwaggerDocument>(filePath);
            importer.UpdateApi(apiId, swagger);
        }

        public void PatchApi(string apiId, string filePath)
        {
            log.InfoFormat("Attempting to patch API from Swagger definition. API identifier: {0} Swagger file: {1}", apiId, filePath);

            var swagger = Import<SwaggerDocument>(filePath);
            importer.PatchApi(apiId, swagger);
        }

        public void Deploy(string apiId, string deploymentConfigFilePath)
        {
            log.InfoFormat("Attempting to deploy API. API identifier: {0}" , apiId);

            var config = Import<DeploymentConfig>(deploymentConfigFilePath);
            importer.Deploy(apiId, config);
        }

        public void DeleteApi(string apiId)
        {
            log.InfoFormat("Attempting to delete API. API identifier: {0}", apiId);

            importer.DeleteApi(apiId);
        }

        public string ProvisionApiKey(string apiId, string name, string stage)
        {
            log.InfoFormat("Attempting to provision API Key. API identifier: {0}", apiId);

            return importer.ProvisionApiKey(apiId, name, stage);
        }

        private static T Import<T>(string filePath)
        {
            var serializer = new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver(), NullValueHandling = NullValueHandling.Ignore };
    
            var sr = new StreamReader(filePath);
            return serializer.Deserialize<T>(new JsonTextReader(sr));
        }
    }
}

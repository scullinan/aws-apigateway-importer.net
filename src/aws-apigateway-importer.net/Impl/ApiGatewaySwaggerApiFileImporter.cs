using System.IO;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace aws_apigateway_importer.net.Impl
{
    public class ApiGatewaySwaggerApiFileImporter : ISwaggerApiFileImporter
    {
        private readonly ISwaggerApiImporter importer;
        ILog log = LogManager.GetLogger(typeof(ApiGatewaySwaggerApiFileImporter));

        public ApiGatewaySwaggerApiFileImporter(ISwaggerApiImporter importer)
        {
            this.importer = importer;
        }

        public async Task<string> ImportApi(string filePath)
        {
            log.InfoFormat("Attempting to create API from Swagger definition. Swagger file: {0}", filePath);

            var swagger = Import(filePath);
            return await importer.CreateApi(swagger, Path.GetFileName(filePath));
        }

        public async Task UpdateApi(string apiId, string filePath)
        {
            log.InfoFormat("Attempting to update API from Swagger definition. API identifier: {0} Swagger file: {1}", apiId, filePath);

            var swagger = Import(filePath);
            await importer.UpdateApi(apiId, swagger);
        }

        public async Task Deploy(string apiId, string deploymentStage)
        {
            await importer.Deploy(apiId, deploymentStage);
        }

        public async Task DeleteApi(string apiId)
        {
            await importer.DeleteApi(apiId);
        }

        private static SwaggerDocument Import(string filePath)
        {
            var serializer = new JsonSerializer {ContractResolver = new CamelCasePropertyNamesContractResolver(), NullValueHandling = NullValueHandling.Ignore };
    
            var sr = new StreamReader(filePath);
            return serializer.Deserialize<SwaggerDocument>(new JsonTextReader(sr));
        }
    }
}

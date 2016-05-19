 using System.Collections.Generic;
 using APIGateway.Swagger;

namespace APIGateway.Management
{
    public interface IApiGatewayProvider
    {
        SwaggerDocument Get(string apiId);
        string Create(string apiName, SwaggerDocument swagger);
        void Update(string apiId, SwaggerDocument swagger);
        void Merge(string apiId, SwaggerDocument swagger);
        void Delete(string apiId);
        void Destory(string apiId);
        void Deploy(string apiId, DeploymentDocument config);
        string Export(string apiId, string stageName, string exportType = "swagger", string accepts = "application/json");

        SwaggerDocument Combine(List<SwaggerDocument> documents);

        string CreateApiKey(string apiId, string keyName, string stageName);
        void DeleteApiKey(string keyName);

        IDictionary<string, string> ListApis();
        IDictionary<string, Key> ListKeys();
        IEnumerable<string> ListOperations(string apiId);
    }
}
    
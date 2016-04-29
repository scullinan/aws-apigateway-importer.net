using System.Collections.Generic;
using Amazon.APIGateway.Model;
using APIGateway.Swagger;

namespace APIGateway.Management
{
    public interface IApiGatewayModelProvider
    {
        void CreateModels(RestApi api, SwaggerDocument swagger);
        void CreateModel(RestApi api, string modelName, Schema model, IDictionary<string, Schema> definitions, string modelContentType);
        void DeleteModels(RestApi api);
        void UpdateModels(RestApi api, SwaggerDocument swagger);
        void CleanupModels(RestApi api);
        IModelNameResolver NameResolver { get; }
    }
}
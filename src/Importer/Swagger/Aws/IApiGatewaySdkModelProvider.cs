using System.Collections.Generic;
using Amazon.APIGateway.Model;

namespace Importer.Swagger.Aws
{
    public interface IApiGatewaySdkModelProvider
    {
        void CreateModels(RestApi api, SwaggerDocument swagger);
        void CreateModel(RestApi api, string modelName, Schema model, IDictionary<string, Schema> definitions, string modelContentType);
        void DeleteDefaultModels(RestApi api);
    }
}
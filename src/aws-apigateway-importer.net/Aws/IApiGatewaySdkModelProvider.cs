using System.Collections.Generic;
using Amazon.APIGateway.Model;
using Importer.Swagger;

namespace Importer.Aws
{
    public interface IApiGatewaySdkModelProvider
    {
        void CreateModels(RestApi api, IDictionary<string, Schema> definitions, IList<string> produces);
        void CreateModel(RestApi api, string modelName, Schema model, IDictionary<string, Schema> definitions, string modelContentType);
        void DeleteDefaultModels(RestApi api);
    }
}
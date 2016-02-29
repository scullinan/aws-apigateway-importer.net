using System;
using System.Threading.Tasks;

namespace AWS.APIGateway
{
    public interface ISwaggerApiImporter
    {
        string CreateApi(SwaggerDocument swagger, String name);
        void UpdateApi(string apiId, SwaggerDocument swagger);
        void Deploy(string apiId, string deploymentStage);
        void DeleteApi(string apiId);
    }
}
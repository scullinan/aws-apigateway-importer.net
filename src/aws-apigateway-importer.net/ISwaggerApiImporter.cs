using System;
using System.Threading.Tasks;

namespace AWS.APIGateway
{
    public interface ISwaggerApiImporter
    {
        string CreateApi(SwaggerDocument swagger, String name);
        void UpdateApi(string apiId, SwaggerDocument swagger);
        void Deploy(string apiId, DeploymentConfig config);
        void DeleteApi(string apiId);
    }
}
using System;

namespace Importer.Swagger
{
    public interface ISwaggerApiImporter
    {
        string CreateApi(SwaggerDocument swagger, String name);
        void UpdateApi(string apiId, SwaggerDocument swagger);
        void Deploy(string apiId, DeploymentConfig config);
        void DeleteApi(string apiId);
        string ProvisionApiKey(string apiId, string name, string stage);
    }
}
using System;
using System.Collections.Generic;

namespace Importer.Swagger
{
    public interface ISwaggerApiImporter
    {
        string CreateApi(SwaggerDocument swagger, String name);
        void UpdateApi(string apiId, SwaggerDocument swagger);
        void MergeApi(string apiId, SwaggerDocument swagger);
        void Deploy(string apiId, DeploymentConfig config);
        void DeleteApi(string apiId);
        string ProvisionApiKey(string apiId, string name, string stage);
        IDictionary<string, string> ListApis();
        IDictionary<string, Key> ListKeys();
    }
}
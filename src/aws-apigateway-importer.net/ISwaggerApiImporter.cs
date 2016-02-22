using System;
using System.Threading.Tasks;

namespace aws_apigateway_importer.net
{
    public interface ISwaggerApiImporter
    {
        Task<string> CreateApi(SwaggerDocument swagger, String name);
        Task UpdateApi(string apiId, SwaggerDocument swagger);
        Task Deploy(string apiId, string deploymentStage);
        Task DeleteApi(string apiId);
    }
}
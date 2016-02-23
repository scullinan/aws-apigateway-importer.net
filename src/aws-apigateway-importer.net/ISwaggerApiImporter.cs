using System;
using System.Threading.Tasks;

namespace AWS.APIGateway
{
    public interface ISwaggerApiImporter
    {
        Task<string> CreateApi(SwaggerDocument swagger, String name);
        Task UpdateApi(string apiId, SwaggerDocument swagger);
        Task Deploy(string apiId, string deploymentStage);
        Task DeleteApi(string apiId);
    }
}
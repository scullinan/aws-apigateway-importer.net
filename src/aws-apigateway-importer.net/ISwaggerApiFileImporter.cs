using System.Threading.Tasks;

namespace aws_apigateway_importer.net
{
    public interface ISwaggerApiFileImporter
    {
        Task<string> ImportApi(string filePath);
        Task UpdateApi(string apiId, string filePath);
        Task Deploy(string apiId, string deploymentStage);
        Task DeleteApi(string apiId);
    }
}
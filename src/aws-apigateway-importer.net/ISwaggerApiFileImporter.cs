using System.Threading.Tasks;

namespace AWS.APIGateway
{
    public interface ISwaggerApiFileImporter
    {
        string ImportApi(string filePath);
        void UpdateApi(string apiId, string filePath);
        void Deploy(string apiId, string deploymentConfigFilePath);
        void DeleteApi(string apiId);
    }
}
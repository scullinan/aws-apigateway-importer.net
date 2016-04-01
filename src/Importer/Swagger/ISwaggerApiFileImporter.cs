namespace Importer.Swagger
{
    public interface ISwaggerApiFileImporter
    {
        string ImportApi(string filePath);
        void UpdateApi(string apiId, string filePath);
        void PatchApi(string apiId, string filePath);
        void Deploy(string apiId, string deploymentConfigFilePath);
        void DeleteApi(string apiId);
        string ProvisionApiKey(string apiId, string name, string stage);
    }
}
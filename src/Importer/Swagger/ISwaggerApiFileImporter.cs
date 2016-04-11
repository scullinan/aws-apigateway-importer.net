using System.Collections.Generic;

namespace Importer.Swagger
{
    public interface ISwaggerApiFileImporter
    {
        string ImportApi(string filePath);
        void UpdateApi(string apiId, string filePath);
        void MergeApi(string apiId, string filePath);
        void Deploy(string apiId, string deploymentConfigFilePath);
        void DeleteApi(string apiId);
        string ProvisionApiKey(string apiId, string name, string stage);
        void DeleteApiKey(string key);
        void WipeApi(string apiId);
        IDictionary<string, string> ListApis();
        IDictionary<string, Key> ListKeys();
    }
}
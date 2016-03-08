using Importer.Aws;
using Importer.Swagger;

namespace Importer
{
    public interface ISwaggerApiImporterFactory
    {
        ISwaggerApiImporter Create(string version = "v2.0");
    }
}
namespace Importer.Swagger
{
    public interface ISwaggerApiImporterFactory
    {
        ISwaggerApiImporter Create(string version = "v2.0");
    }
}
namespace Importer.Swagger.Aws
{
    public interface IModelNameResolver
    {
        string GetModelName(string resourceName, string method, Response response);
        string GetModelName(string refPath);
        string Sanitize(string str);
    }
}
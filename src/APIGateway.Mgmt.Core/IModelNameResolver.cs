using APIGateway.Swagger;

namespace APIGateway.Management
{
    public interface IModelNameResolver
    {
        string GetModelName(string refPath);
        string GetArrayModelName(string refPath);
        string GetModelName(string resourceName, string method, Response response);
        string Sanitize(string str);
    }
}
using System.Collections.Generic;
using Amazon.APIGateway.Model;
using APIGateway.Swagger;

namespace APIGateway.Management
{
    public interface IApiGatewayMethodParameterProvider
    {
        void CreateMethodParameters(RestApi api, Resource resource, Method method, IList<Parameter> parameters);
        void UpdateMethodParameters(RestApi api, Resource resource, Method method, IList<Parameter> parameters);
    }
}
using System.Collections.Generic;
using Amazon.APIGateway.Model;
using Importer.Swagger;

namespace Importer.Aws
{
    public interface IApiGatewaySdkMethodParameterProvider
    {
        void CreateMethodParameters(RestApi api, Resource resource, Method method, IList<Parameter> parameters);
    }
}
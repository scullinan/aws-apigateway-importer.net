using System.Collections.Generic;
using Amazon.APIGateway.Model;

namespace Importer.Swagger.Aws
{
    public interface IApiGatewaySdkMethodParameterProvider
    {
        void CreateMethodParameters(RestApi api, Resource resource, Method method, IList<Parameter> parameters);
    }
}
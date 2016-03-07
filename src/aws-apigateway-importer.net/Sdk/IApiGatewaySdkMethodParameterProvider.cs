using System.Collections.Generic;
using Amazon.APIGateway.Model;

namespace ApiGatewayImporter.Sdk
{
    public interface IApiGatewaySdkMethodParameterProvider
    {
        void CreateMethodParameters(RestApi api, Resource resource, Method method, IList<Parameter> parameters);
    }
}
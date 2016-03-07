using System.Collections.Generic;
using Amazon.APIGateway.Model;

namespace ApiGatewayImporter.Sdk
{
    public interface IApiGatewaySdkMethodProvider
    {
        
    }

    public interface IApiGatewaySdkModelProvider
    {
        void CreateModels(RestApi api, IDictionary<string, Schema> definitions, IList<string> produces);
    }

    public interface IApiGatewaySdkResourceProvider
    {
    }

    public interface IApiGatewaySdkDeploymentProvider
    {
    }

    public interface IApiGatewaySdkDomainProvider
    {
    }
}

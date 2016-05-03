using System.Collections.Generic;
using Amazon;
using Amazon.APIGateway;

namespace APIGateway.Management.Impl
{
    public class ApiGatewayProviderFactory : IApiGatewayProviderFactory
    {
        private readonly HashSet<string> processedModels = new HashSet<string>();

        public static IApiGatewayProviderFactory Instance()
        {
            return new ApiGatewayProviderFactory();
        }

        public IApiGatewayProvider Create()
        {
            var gateway = CreateClient();

            var modelNameResolver = new ModelNameResolver();
            var modelProvider = new ApiGatewayModelProvider(processedModels, gateway, modelNameResolver);


            var methodResponseProvider = new ApiGatewayMethodResponseProvider(processedModels, gateway, modelProvider);
            var methodParameterProvider = new ApiGatewayMethodParameterProvider(gateway);
            var methodIntegrationProvider = new ApiGatewayMethodIntegrationProvider(gateway);
            var methodProvider = new ApiGatewayMethodProvider(processedModels, gateway, modelProvider, methodResponseProvider, methodParameterProvider, methodIntegrationProvider);

            var resourceProvider = new ApiGatewayResourceProvider(gateway, methodProvider);
            var deploymentProvider = new ApiGatewayDeploymentProvider(gateway);

            return new ApiGatewayProvider(gateway, modelProvider, resourceProvider, methodProvider, deploymentProvider);
        }

        public IEnumerable<string> Models => processedModels;

        private IAmazonAPIGateway CreateClient()
        {
            AWSConfigs.LoggingConfig.LogTo = LoggingOptions.Log4Net;
            return new AmazonAPIGatewayClient();
        }
    }
}
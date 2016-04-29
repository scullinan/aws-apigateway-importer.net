using System;
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

        public IApiGatewayProvider Create(string version = "v2.0")
        {
            switch (version)
            {
                case "v2.0" :
                {
                    return CreateSwaggerV20();
                }
                default:
                    throw new NotSupportedException($"Specified version {version} not supported");
            }
        }

        public IEnumerable<string> Models => processedModels;

        private ApiGatewayProvider CreateSwaggerV20()
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

        private IAmazonAPIGateway CreateClient()
        {
            AWSConfigs.LoggingConfig.LogTo = LoggingOptions.Log4Net;
            return new AmazonAPIGatewayClient();
        }
    }
}
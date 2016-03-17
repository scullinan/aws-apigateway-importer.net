using System;
using System.Collections.Generic;
using Amazon;
using Amazon.APIGateway;
using Amazon.Runtime;

namespace Importer.Swagger.Aws.Impl
{
    public class ApiGatewaySdkSwaggerApiImporterFactory : ISwaggerApiImporterFactory
    {
        private readonly HashSet<string> processedModels = new HashSet<string>();

        public static ISwaggerApiImporterFactory Instance()
        {
            return new ApiGatewaySdkSwaggerApiImporterFactory();
        }

        public ISwaggerApiImporter Create(string version = "v2.0")
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

        private ISwaggerApiImporter CreateSwaggerV20()
        {
            var gateway = CreateClient();

            var modelNameResolver = new ModelNameResolver();
            var modelProvider = new ApiGatewaySdkModelProvider(processedModels, gateway, modelNameResolver);
           

            var methodResponseProvider = new ApiGatewaySdkMethodResponseProvider(processedModels, gateway, modelProvider);
            var methodParameterProvider = new ApiGatewaySdkMethodParameterProvider(gateway);
            var methodIntegrationProvider = new ApiGatewaySdkMethodIntegrationProvider(gateway);
            var methodProvider = new ApiGatewaySdkMethodProvider(processedModels, gateway, modelProvider, methodResponseProvider, methodParameterProvider, methodIntegrationProvider);

            var resourceProvider = new ApiGatewaySdkResourceProvider(gateway, methodProvider);
            var deploymentProvider = new ApiGatewaySdkDeploymentProvider(gateway);

            return new ApiGatewaySdkSwaggerApiImporter(gateway, modelProvider, resourceProvider, methodProvider, deploymentProvider);
        }

        private IAmazonAPIGateway CreateClient()
        {
            AWSConfigs.LoggingConfig.LogTo = LoggingOptions.Log4Net;
            AWSConfigs.LoggingConfig.LogMetrics = true;
            AWSConfigs.LoggingConfig.LogResponses = ResponseLoggingOption.OnError;
            return new AmazonAPIGatewayClient();
        }
    }
}
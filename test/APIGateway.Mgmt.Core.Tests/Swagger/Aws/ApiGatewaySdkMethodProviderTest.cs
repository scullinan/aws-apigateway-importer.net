using System.Collections.Generic;
using System.Linq;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using Importer.Swagger;
using Importer.Swagger.Aws;
using Importer.Swagger.Aws.Impl;
using Moq;
using NUnit.Framework;

namespace Importer.Tests.Swagger.Aws
{
    [TestFixture]
    public class ApiGatewaySdkMethodProviderTest
    {
        private ApiGatewaySdkMethodProvider underTest;
        private HashSet<string> processedModels;
        private Mock<IAmazonAPIGateway> gatewayMock;
        private Mock<IApiGatewaySdkModelProvider> modelProviderMock;
        private Mock<IApiGatewaySdkMethodResponseProvider> methodResponseProviderMock;
        private Mock<IApiGatewaySdkMethodParameterProvider> methodParameterProviderMock;
        private Mock<IApiGatewaySdkMethodIntegrationProvider> methodIntegrationProviderMock;
        private string apiId = "pwfy7quvxh";

        public ApiGatewaySdkMethodProviderTest()
        {
            processedModels = new HashSet<string>();
            gatewayMock = new Mock<IAmazonAPIGateway>();
            modelProviderMock = new Mock<IApiGatewaySdkModelProvider>();
            methodResponseProviderMock = new Mock<IApiGatewaySdkMethodResponseProvider>();
            methodParameterProviderMock = new Mock<IApiGatewaySdkMethodParameterProvider>();
            methodIntegrationProviderMock = new Mock<IApiGatewaySdkMethodIntegrationProvider>();

            underTest = new ApiGatewaySdkMethodProvider(
                processedModels, 
                gatewayMock.Object, 
                modelProviderMock.Object, 
                methodResponseProviderMock.Object, 
                methodParameterProviderMock.Object,
                methodIntegrationProviderMock.Object);
        }

        [Test]
        public void CreateMethodsTest()
        {
            var swagger = TestHelper.Import<SwaggerDocument>("apigateway.json");
            var restApi = new RestApi() { Id = apiId };
            var rootResource = TestHelper.GetRootResource();
            var pathItem = swagger.Paths.FirstOrDefault();

            gatewayMock.Setup(x => x.PutMethod(It.IsAny<PutMethodRequest>())).Returns(new PutMethodResponse());

            underTest.CreateMethods(restApi, swagger, rootResource, pathItem.Value, swagger.Produces);

            methodResponseProviderMock.Verify(x => x.CreateMethodResponses(restApi, rootResource, It.IsAny<Method>(), swagger, It.IsAny<string>(), It.IsAny<IDictionary<string, Response>>()), Times.Once);
            methodParameterProviderMock.Verify(x => x.CreateMethodParameters(restApi, rootResource, It.IsAny<Method>(), It.IsAny<List<Parameter>>()), Times.Once);
            methodIntegrationProviderMock.Verify(x => x.CreateIntegration(restApi, rootResource, It.IsAny<Method>(), It.IsAny<IDictionary<string, object>>()), Times.Once);
        }
    }
}
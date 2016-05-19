using System.Collections.Generic;
using System.Linq;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using APIGateway.Management;
using APIGateway.Management.Impl;
using APIGateway.Swagger;
using Moq;
using NUnit.Framework;

namespace APIGateway.Mgmt.Core.Tests.Swagger.Aws
{
    [TestFixture]
    public class ApiGatewayMethodProviderTest
    {
        private ApiGatewayMethodProvider underTest;
        private HashSet<string> processedModels;
        private Mock<IAmazonAPIGateway> gatewayMock;
        private Mock<IApiGatewayModelProvider> modelProviderMock;
        private Mock<IApiGatewayMethodResponseProvider> methodResponseProviderMock;
        private Mock<IApiGatewayMethodParameterProvider> methodParameterProviderMock;
        private Mock<IApiGatewayMethodIntegrationProvider> methodIntegrationProviderMock;
        private string apiId = "pwfy7quvxh";

        public ApiGatewayMethodProviderTest()
        {
            processedModels = new HashSet<string>();
            gatewayMock = new Mock<IAmazonAPIGateway>();
            modelProviderMock = new Mock<IApiGatewayModelProvider>();
            methodResponseProviderMock = new Mock<IApiGatewayMethodResponseProvider>();
            methodParameterProviderMock = new Mock<IApiGatewayMethodParameterProvider>();
            methodIntegrationProviderMock = new Mock<IApiGatewayMethodIntegrationProvider>();

            underTest = new ApiGatewayMethodProvider(
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
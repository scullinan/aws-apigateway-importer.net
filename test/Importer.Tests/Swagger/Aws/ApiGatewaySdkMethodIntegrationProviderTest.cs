using System.Linq;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using Importer.Swagger;
using Importer.Swagger.Aws.Impl;
using Moq;
using NUnit.Framework;

namespace Importer.Tests.Swagger.Aws
{
    [TestFixture]
    public class ApiGatewaySdkMethodIntegrationProviderTest
    {
        private ApiGatewaySdkMethodIntegrationProvider underTest;
        private Mock<IAmazonAPIGateway> gatewayMock;
        private string apiId = "pwfy7quvxh";

        public ApiGatewaySdkMethodIntegrationProviderTest()
        {
            gatewayMock = new Mock<IAmazonAPIGateway>();
            underTest = new ApiGatewaySdkMethodIntegrationProvider(gatewayMock.Object);
        }

        [Test]
        public void CreateIntegrationTest()
        {
            var swagger = TestHelper.Import<SwaggerDocument>("apigateway.json");
            var restApi = new RestApi() { Id = apiId };
            var rootResource = TestHelper.GetRootResource();
            var method = new Method() { HttpMethod = "Get" };
            var operation = swagger.Paths.FirstOrDefault().Value.Get;

            gatewayMock.Setup(x => x.PutIntegration(It.IsAny<PutIntegrationRequest>()))
                .Returns(new PutIntegrationResponse());

            underTest.CreateIntegration(restApi, rootResource, method, operation.VendorExtensions);

            gatewayMock.Verify(x => x.PutIntegration(It.IsAny<PutIntegrationRequest>()), Times.Once);
            gatewayMock.Verify(x => x.PutIntegrationResponse(It.IsAny<PutIntegrationResponseRequest>()), Times.Exactly(2));
        }
    }
}
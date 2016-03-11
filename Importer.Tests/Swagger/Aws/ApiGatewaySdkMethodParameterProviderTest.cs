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
    public class ApiGatewaySdkMethodParameterProviderTest
    {
        private ApiGatewaySdkMethodParameterProvider underTest;
        private Mock<IAmazonAPIGateway> gatewayMock;
        private string apiId = "pwfy7quvxh";

        public ApiGatewaySdkMethodParameterProviderTest()
        {
            gatewayMock = new Mock<IAmazonAPIGateway>();
            underTest = new ApiGatewaySdkMethodParameterProvider(gatewayMock.Object);
        }

        [Test]
        public void CreateMethodParametersTest()
        {
            var swagger = TestHelper.Import<SwaggerDocument>("apigateway.json");
            var restApi = new RestApi() { Id = apiId };
            var rootResource = TestHelper.GetRootResource();
            var method = new Method() {HttpMethod = "Get"};

            var operation = swagger.Paths.FirstOrDefault().Value.Get;

            underTest.CreateMethodParameters(restApi, rootResource, method, operation.Parameters);

            gatewayMock.Verify(x => x.UpdateMethod(It.IsAny<UpdateMethodRequest>()), Times.Exactly(2));
        }
    }
}
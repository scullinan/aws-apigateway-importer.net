using System.Linq;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using APIGateway.Management.Impl;
using APIGateway.Swagger;
using Moq;
using NUnit.Framework;

namespace APIGateway.Mgmt.Core.Tests.Swagger.Aws
{
    [TestFixture]
    public class ApiGatewayMethodParameterProviderTest
    {
        private ApiGatewayMethodParameterProvider underTest;
        private Mock<IAmazonAPIGateway> gatewayMock;
        private string apiId = "pwfy7quvxh";

        public ApiGatewayMethodParameterProviderTest()
        {
            gatewayMock = new Mock<IAmazonAPIGateway>();
            gatewayMock.Setup(x => x.GetModel(It.IsAny<GetModelRequest>())).Returns(new GetModelResponse());
            underTest = new ApiGatewayMethodParameterProvider(gatewayMock.Object);
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
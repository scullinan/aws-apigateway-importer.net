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
    public class ApiGatewayMethodResponseProviderTest
    {
        private ApiGatewayMethodResponseProvider underTest;
        private HashSet<string> processedModels;
        private Mock<IAmazonAPIGateway> gatewayMock;
        private Mock<IApiGatewayModelProvider> modelProviderMock;
        private Mock<ModelNameResolver> modelNameResolverMock;
        private string apiId = "pwfy7quvxh";

        public ApiGatewayMethodResponseProviderTest()
        {
            processedModels = new HashSet<string>();
            gatewayMock = new Mock<IAmazonAPIGateway>();
            modelProviderMock = new Mock<IApiGatewayModelProvider>();
            modelNameResolverMock = new Mock<ModelNameResolver>();
            gatewayMock.Setup(x => x.GetModel(It.IsAny<GetModelRequest>())).Returns(new GetModelResponse());

            modelProviderMock.Setup(x => x.NameResolver).Returns(modelNameResolverMock.Object);
            underTest = new ApiGatewayMethodResponseProvider(processedModels, gatewayMock.Object, modelProviderMock.Object);
        }

        [Test]
        public void CreateMethodParametersTest()
        {
            var swagger = TestHelper.Import<SwaggerDocument>("apigateway.json");
            var restApi = new RestApi() { Id = apiId };
            var rootResource = TestHelper.GetRootResource();
            var method = new Method() { HttpMethod = "Get" };
            var operation = swagger.Paths.FirstOrDefault().Value.Get;

            underTest.CreateMethodResponses(restApi, rootResource, method, swagger, "application/json", operation.Responses);

            gatewayMock.Verify(x => x.PutMethodResponse(It.IsAny<PutMethodResponseRequest>()), Times.Exactly(2));
        }
    }
}
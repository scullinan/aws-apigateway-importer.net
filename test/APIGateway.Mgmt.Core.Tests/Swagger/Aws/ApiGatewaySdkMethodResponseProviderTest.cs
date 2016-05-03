using System.Collections.Generic;
using System.Linq;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using Amazon.Runtime.Internal.Util;
using Importer.Swagger;
using Importer.Swagger.Aws;
using Importer.Swagger.Aws.Impl;
using Moq;
using NUnit.Framework;

namespace Importer.Tests.Swagger.Aws
{
    [TestFixture]
    public class ApiGatewaySdkMethodResponseProviderTest
    {
        private ApiGatewaySdkMethodResponseProvider underTest;
        private HashSet<string> processedModels;
        private Mock<IAmazonAPIGateway> gatewayMock;
        private Mock<IApiGatewaySdkModelProvider> modelProviderMock;
        private Mock<ModelNameResolver> modelNameResolverMock;
        private string apiId = "pwfy7quvxh";

        public ApiGatewaySdkMethodResponseProviderTest()
        {
            processedModels = new HashSet<string>();
            gatewayMock = new Mock<IAmazonAPIGateway>();
            modelProviderMock = new Mock<IApiGatewaySdkModelProvider>();
            modelNameResolverMock = new Mock<ModelNameResolver>();
            gatewayMock.Setup(x => x.GetModel(It.IsAny<GetModelRequest>())).Returns(new GetModelResponse());

            modelProviderMock.Setup(x => x.NameResolver).Returns(modelNameResolverMock.Object);
            underTest = new ApiGatewaySdkMethodResponseProvider(processedModels, gatewayMock.Object, modelProviderMock.Object);
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
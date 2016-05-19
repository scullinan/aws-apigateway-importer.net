using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using Amazon.Runtime.Internal;
using APIGateway.Management;
using APIGateway.Management.Impl;
using APIGateway.Swagger;
using Moq;
using NUnit.Framework;

namespace APIGateway.Mgmt.Core.Tests.Swagger.Aws
{
    [TestFixture]
    public class ApiGatewayResourceProviderTest
    {
        private ApiGatewayResourceProvider underTest;
        private Mock<IAmazonAPIGateway> gatewayMock;
        private Mock<IApiGatewayMethodProvider> sdkMethodProviderMock;
        private string apiId = "pwfy7quvxh";

        public ApiGatewayResourceProviderTest()
        {
            gatewayMock = new Mock<IAmazonAPIGateway>();
            sdkMethodProviderMock = new Mock<IApiGatewayMethodProvider>();
            underTest = new ApiGatewayResourceProvider(gatewayMock.Object, sdkMethodProviderMock.Object);
        }

        [Test]
        public void CreateResourceTest()
        {
            gatewayMock.Setup(x => x.GetResources(It.IsAny<GetResourcesRequest>()))
                .Returns(new GetResourcesResponse() {Items = new AutoConstructedList<Resource>()});

            gatewayMock.Setup(x => x.CreateResource(It.IsAny<CreateResourceRequest>()))
               .Returns(new CreateResourceResponse() { });

            var swagger = TestHelper.Import<SwaggerDocument>("apigateway.json");
            var restApi = new RestApi() { Id = apiId };
            var rootResource = TestHelper.GetRootResource();

            underTest.CreateResources(restApi, rootResource, swagger, true);
            
            gatewayMock.Verify(x => x.CreateResource(It.IsAny<CreateResourceRequest>()), Times.AtLeast(1));
            sdkMethodProviderMock.Verify(x => x.CreateMethods(It.IsAny<RestApi>(), swagger, It.IsAny<Resource>(), It.IsAny<PathItem>(), swagger.Produces), Times.Exactly(2));
        }

        
    }
}
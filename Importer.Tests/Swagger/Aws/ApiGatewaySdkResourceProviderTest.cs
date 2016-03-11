using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using Amazon.Runtime.Internal;
using Importer.Swagger;
using Importer.Swagger.Aws;
using Importer.Swagger.Aws.Impl;
using Moq;
using NUnit.Framework;

namespace Importer.Tests.Swagger.Aws
{
    [TestFixture]
    public class ApiGatewaySdkResourceProviderTest
    {
        private ApiGatewaySdkResourceProvider underTest;
        private Mock<IAmazonAPIGateway> gatewayMock;
        private Mock<IApiGatewaySdkMethodProvider> sdkMethodProviderMock;
        private string apiId = "pwfy7quvxh";

        public ApiGatewaySdkResourceProviderTest()
        {
            gatewayMock = new Mock<IAmazonAPIGateway>();
            sdkMethodProviderMock = new Mock<IApiGatewaySdkMethodProvider>();
            underTest = new ApiGatewaySdkResourceProvider(gatewayMock.Object, sdkMethodProviderMock.Object);
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
            var rootResource = GetRootResource();

            underTest.CreateResources(restApi, rootResource, swagger, true);
            
            gatewayMock.Verify(x => x.CreateResource(It.IsAny<CreateResourceRequest>()), Times.AtLeast(1));
            sdkMethodProviderMock.Verify(x => x.CreateMethods(It.IsAny<RestApi>(), swagger, It.IsAny<Resource>(), It.IsAny<PathItem>(), swagger.Produces), Times.AtLeast(1));
        }

        private Resource GetRootResource()
        {
            return new Resource() {Id = "id123", Path = "/"};
        }
    }
}
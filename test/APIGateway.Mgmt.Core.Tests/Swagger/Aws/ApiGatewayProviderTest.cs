using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using APIGateway.Management;
using APIGateway.Management.Impl;
using APIGateway.Swagger;
using Moq;
using NUnit.Framework;

namespace APIGateway.Mgmt.Core.Tests.Swagger.Aws
{
    public class ApiGatewayProviderTest
    {
        private ApiGatewayProvider underTest;
        private Mock<IAmazonAPIGateway> gatewayMock;
        private Mock<IApiGatewayModelProvider> sdkModelProviderMock;
        private Mock<IApiGatewayResourceProvider> resourceProviderMock;
        private Mock<IApiGatewayMethodProvider> methodProviderMock;
        private Mock<IApiGatewayDeploymentProvider> deploymentProviderMock;
        private string apiId = "pwfy7quvxh";

        public ApiGatewayProviderTest()
        {
            gatewayMock = new Mock<IAmazonAPIGateway>();
            sdkModelProviderMock = new Mock<IApiGatewayModelProvider>();
            resourceProviderMock = new Mock<IApiGatewayResourceProvider>();
            deploymentProviderMock = new Mock<IApiGatewayDeploymentProvider>();
            methodProviderMock = new Mock<IApiGatewayMethodProvider>();

            underTest = new ApiGatewayProvider(gatewayMock.Object, sdkModelProviderMock.Object, resourceProviderMock.Object, methodProviderMock.Object, deploymentProviderMock.Object);
        }

        [Test]
        public void CreateApiTest()
        {
            var swagger = TestHelper.Import<SwaggerDocument>("apigateway.json");

            gatewayMock.Setup(x => x.CreateRestApi(It.IsAny<CreateRestApiRequest>())).Returns(new CreateRestApiResponse() { Id = apiId });
            gatewayMock.Setup(x => x.GetResources(It.IsAny<GetResourcesRequest>())).Returns(new GetResourcesResponse());


            var result = underTest.Create("api", swagger);

            gatewayMock.Verify(x => x.CreateRestApi(It.IsAny<CreateRestApiRequest>()), Times.Once);
            sdkModelProviderMock.Verify(x => x.DeleteModels(It.IsAny<RestApi>()), Times.Once);
            sdkModelProviderMock.Verify(x => x.CreateModels(It.IsAny<RestApi>(), It.IsAny<SwaggerDocument>()), Times.Once);
            resourceProviderMock.Verify(x => x.CreateResources(It.IsAny<RestApi>(), It.IsAny<Resource>(), It.IsAny<SwaggerDocument>(), true), Times.Once);

            Assert.That(apiId, Is.EqualTo(result));
        }

        [Test]
        public void DeleteApiTest()
        {
            underTest.Delete(apiId);
            gatewayMock.Verify(x => x.DeleteRestApi(It.IsAny<DeleteRestApiRequest>()), Times.Once);
        }

        [Test]
        public void DeployApiTest()
        {
            var deploymentConfig = TestHelper.Import<DeploymentDocument>("deployment.json");

            underTest.Deploy(apiId, deploymentConfig);

            deploymentProviderMock.Verify(x => x.CreateDeployment(apiId, deploymentConfig), Times.Once);
        }

        [Test]
        public void ProvisionApiKeyTest()
        {
            underTest.CreateApiKey(apiId, It.IsAny<string>(), It.IsAny<string>());
            deploymentProviderMock.Verify(x => x.CreateApiKey(apiId, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}

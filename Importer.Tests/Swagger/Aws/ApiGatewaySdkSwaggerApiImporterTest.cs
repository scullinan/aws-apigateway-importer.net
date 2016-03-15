using System.IO;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using Importer.Swagger;
using Importer.Swagger.Aws;
using Importer.Swagger.Aws.Impl;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace Importer.Tests.Swagger.Aws
{
    public class ApiGatewaySdkSwaggerApiImporterTest
    {
        private ApiGatewaySdkSwaggerApiImporter underTest;
        private Mock<IAmazonAPIGateway> gatewayMock;
        private Mock<IApiGatewaySdkModelProvider> sdkModelProviderMock;
        private Mock<IApiGatewaySdkResourceProvider> resourceProviderMock;
        private Mock<IApiGatewaySdkMethodProvider> methodProviderMock;
        private Mock<IApiGatewaySdkDeploymentProvider> deploymentProviderMock;
        private string apiId = "pwfy7quvxh";

        public ApiGatewaySdkSwaggerApiImporterTest()
        {
            gatewayMock = new Mock<IAmazonAPIGateway>();
            sdkModelProviderMock = new Mock<IApiGatewaySdkModelProvider>();
            resourceProviderMock = new Mock<IApiGatewaySdkResourceProvider>();
            deploymentProviderMock = new Mock<IApiGatewaySdkDeploymentProvider>();
            methodProviderMock = new Mock<IApiGatewaySdkMethodProvider>();

            underTest = new ApiGatewaySdkSwaggerApiImporter(gatewayMock.Object, sdkModelProviderMock.Object, resourceProviderMock.Object, methodProviderMock.Object, deploymentProviderMock.Object);
        }

        [Test]
        public void CreateApiTest()
        {
            var swagger = TestHelper.Import<SwaggerDocument>("apigateway.json");

            gatewayMock.Setup(x => x.CreateRestApi(It.IsAny<CreateRestApiRequest>())).Returns(new CreateRestApiResponse() { Id = apiId });
            gatewayMock.Setup(x => x.GetResources(It.IsAny<GetResourcesRequest>())).Returns(new GetResourcesResponse());


            var result = underTest.CreateApi(swagger, "api");

            gatewayMock.Verify(x => x.CreateRestApi(It.IsAny<CreateRestApiRequest>()), Times.Once);
            sdkModelProviderMock.Verify(x => x.DeleteDefaultModels(It.IsAny<RestApi>()), Times.Once);
            sdkModelProviderMock.Verify(x => x.CreateModels(It.IsAny<RestApi>(), It.IsAny<SwaggerDocument>()), Times.Once);
            resourceProviderMock.Verify(x => x.CreateResources(It.IsAny<RestApi>(), It.IsAny<Resource>(), It.IsAny<SwaggerDocument>(), true), Times.Once);

            Assert.That(apiId, Is.EqualTo(result));
        }

        [Test]
        public void DeleteApiTest()
        {
            underTest.DeleteApi(apiId);
            gatewayMock.Verify(x => x.DeleteRestApi(It.IsAny<DeleteRestApiRequest>()), Times.Once);
        }

        [Test]
        public void DeployApiTest()
        {
            var deploymentConfig = TestHelper.Import<DeploymentConfig>("deployment.json");

            underTest.Deploy(apiId, deploymentConfig);

            deploymentProviderMock.Verify(x => x.CreateDeployment(apiId, deploymentConfig), Times.Once);
        }

        [Test]
        public void ProvisionApiKeyTest()
        {
            underTest.ProvisionApiKey(apiId, It.IsAny<string>(), It.IsAny<string>());
            deploymentProviderMock.Verify(x => x.CreateApiKey(apiId, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        
    }
}

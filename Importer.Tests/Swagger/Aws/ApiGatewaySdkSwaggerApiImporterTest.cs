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
        private Mock<IApiGatewaySdkDeploymentProvider> deploymentProviderMock;
        private string apiId = "pwfy7quvxh";

        public ApiGatewaySdkSwaggerApiImporterTest()
        {
            gatewayMock = new Mock<IAmazonAPIGateway>();
            sdkModelProviderMock = new Mock<IApiGatewaySdkModelProvider>();
            resourceProviderMock = new Mock<IApiGatewaySdkResourceProvider>();
            deploymentProviderMock = new Mock<IApiGatewaySdkDeploymentProvider>();

            underTest = new ApiGatewaySdkSwaggerApiImporter(gatewayMock.Object, sdkModelProviderMock.Object, resourceProviderMock.Object, deploymentProviderMock.Object);
        }

        [Test]
        public void CreateApiTest()
        {
            var filePath = TestHelper.GetResourceFilePath("apigateway.json");
            var swagger = Import<SwaggerDocument>(filePath);

            gatewayMock.Setup(x => x.CreateRestApi(It.IsAny<CreateRestApiRequest>())).Returns(new CreateRestApiResponse() { Id = apiId });
            gatewayMock.Setup(x => x.GetResources(It.IsAny<GetResourcesRequest>())).Returns(new GetResourcesResponse());


            var result = underTest.CreateApi(swagger, "api");

            gatewayMock.Verify(x => x.CreateRestApi(It.IsAny<CreateRestApiRequest>()), Times.Once);
            sdkModelProviderMock.Verify(x => x.DeleteDefaultModels(It.IsAny<RestApi>()), Times.Once);
            sdkModelProviderMock.Verify(x => x.CreateModels(It.IsAny<RestApi>(), It.IsAny<SwaggerDocument>()), Times.Once);
            resourceProviderMock.Verify(x => x.CreateResources(It.IsAny<RestApi>(), It.IsAny<Resource>(), It.IsAny<SwaggerDocument>(), true), Times.Once);

            Assert.That(apiId, Is.EqualTo(result));
        }

        private static T Import<T>(string filePath)
        {
            var serializer = new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver(), NullValueHandling = NullValueHandling.Ignore };

            var sr = new StreamReader(filePath);
            return serializer.Deserialize<T>(new JsonTextReader(sr));
        }
    }
}

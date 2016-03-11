using System.Collections.Generic;
using System.Linq;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using Importer.Swagger;
using Importer.Swagger.Aws;
using Importer.Swagger.Aws.Impl;
using Moq;
using NUnit.Framework;

namespace Importer.Tests.Swagger.Aws
{
    [TestFixture]
    public class ApiGatewaySdkModelProviderTest
    {
        private ApiGatewaySdkModelProvider underTest;
        private Mock<IAmazonAPIGateway> gatewayMock;
        private string apiId = "pwfy7quvxh";
        private HashSet<string> processedModels;

        public ApiGatewaySdkModelProviderTest()
        {
            processedModels = new HashSet<string>();
            gatewayMock = new Mock<IAmazonAPIGateway>();

            underTest = new ApiGatewaySdkModelProvider(processedModels, gatewayMock.Object);
        }

        [Test]
        public void CreateModelsTest()
        {
            var swagger = TestHelper.Import<SwaggerDocument>("apigateway.json");
            var restApi = new RestApi() { Id = apiId };

            underTest.CreateModels(restApi, swagger);
            gatewayMock.Verify(x => x.CreateModel(It.IsAny<CreateModelRequest>()), Times.Exactly(6));
        }

        [Test]
        public void CreateModelTest()
        {
            var swagger = TestHelper.Import<SwaggerDocument>("apigateway.json");
            var restApi = new RestApi() { Id = apiId };

            //Re Init
            processedModels = new HashSet<string>();
            gatewayMock = new Mock<IAmazonAPIGateway>();
            underTest = new ApiGatewaySdkModelProvider(processedModels, gatewayMock.Object);

            var definition = swagger.Definitions.FirstOrDefault();
            var modelName = definition.Key;
            var model = definition.Value;

            underTest.CreateModel(restApi, modelName, model, swagger.Definitions, "application/json");
            gatewayMock.Verify(x => x.CreateModel(It.IsAny<CreateModelRequest>()), Times.Once);
        }

        [Test]
        public void DeleteDefaultModelsTest()
        {
            gatewayMock.Setup(x => x.GetModels(It.IsAny<GetModelsRequest>())).Returns(new GetModelsResponse() { Items = new List<Model>() {
                new Model() {Name = "model1" },
                new Model() {Name = "model2" }
            }});

            var restApi = new RestApi() { Id = apiId };
            underTest.DeleteDefaultModels(restApi);

            gatewayMock.Verify(x => x.DeleteModel(It.IsAny<DeleteModelRequest>()), Times.Exactly(2));
        }
    }
}
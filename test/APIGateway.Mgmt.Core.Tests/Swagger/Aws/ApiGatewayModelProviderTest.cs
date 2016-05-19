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
    public class ApiGatewayModelProviderTest
    {
        private ApiGatewayModelProvider underTest;
        private Mock<IAmazonAPIGateway> gatewayMock;
        private Mock<IModelNameResolver> resolverMock;
        private string apiId = "pwfy7quvxh";
        private HashSet<string> processedModels;

        public ApiGatewayModelProviderTest()
        {
            processedModels = new HashSet<string>();
            gatewayMock = new Mock<IAmazonAPIGateway>();
            resolverMock = new Mock<IModelNameResolver>();
            gatewayMock.Setup(x => x.GetModel(It.IsAny<GetModelRequest>())).Returns(new GetModelResponse());
            resolverMock.Setup(x => x.Sanitize(It.IsAny<string>())).Returns("model_name");
            underTest = new ApiGatewayModelProvider(processedModels, gatewayMock.Object, resolverMock.Object);
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
            resolverMock = new Mock<IModelNameResolver>();
            underTest = new ApiGatewayModelProvider(processedModels, gatewayMock.Object, resolverMock.Object);

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
                new Model() { Name = "model1" },
                new Model() { Name = "model2" }
            }});

            var restApi = new RestApi() { Id = apiId };
            underTest.DeleteModels(restApi);

            gatewayMock.Verify(x => x.DeleteModel(It.IsAny<DeleteModelRequest>()), Times.Exactly(2));
        }
    }
}

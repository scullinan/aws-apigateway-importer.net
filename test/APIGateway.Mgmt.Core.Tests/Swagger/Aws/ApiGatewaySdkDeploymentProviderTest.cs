using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using Importer.Swagger.Aws.Impl;
using Moq;
using NUnit.Framework;

namespace Importer.Tests.Swagger.Aws
{
    [TestFixture]
    public class ApiGatewaySdkDeploymentProviderTest
    {
        private ApiGatewaySdkDeploymentProvider underTest;
        private Mock<IAmazonAPIGateway> gatewayMock;
        private string apiId = "pwfy7quvxh";

        public ApiGatewaySdkDeploymentProviderTest()
        {
            gatewayMock = new Mock<IAmazonAPIGateway>();
            underTest = new ApiGatewaySdkDeploymentProvider(gatewayMock.Object);
        }

        [Test]
        public void CreateDeploymentTest()
        {
            var config = TestHelper.Import<DeploymentConfig>("deployment.json");
            
            gatewayMock.Setup(x => x.PutIntegration(It.IsAny<PutIntegrationRequest>()))
                .Returns(new PutIntegrationResponse());

            underTest.CreateDeployment(apiId, config);

            gatewayMock.Verify(x => x.CreateDeployment(It.IsAny<CreateDeploymentRequest>()), Times.Once);
            gatewayMock.Verify(x => x.UpdateStage(It.IsAny<UpdateStageRequest>()), Times.Once);
        }

        [Test]
        public void CreateDomainTest()
        {
            var config = TestHelper.Import<DeploymentConfig>("deployment.json");

            underTest.CreateDomain(config);
            gatewayMock.Verify(x => x.CreateDomainName(It.IsAny<CreateDomainNameRequest>()), Times.Once);
        }

        [Test]
        public void CreateApiKeyTest()
        {
            var key = "590e100a45a0437d9bc23ad332f5ccee";

            gatewayMock.Setup(x => x.CreateApiKey(It.IsAny<CreateApiKeyRequest>()))
               .Returns(new CreateApiKeyResponse() { Id = key });

            var apiKey = underTest.CreateApiKey(apiId, "apikey-name", "stage-name");

            Assert.That(apiKey, Is.EqualTo(key));
            gatewayMock.Verify(x => x.CreateApiKey(It.IsAny<CreateApiKeyRequest>()), Times.Once);
        }
    }
}
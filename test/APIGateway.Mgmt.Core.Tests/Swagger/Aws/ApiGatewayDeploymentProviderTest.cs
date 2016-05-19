using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using APIGateway.Management;
using APIGateway.Management.Impl;
using Moq;
using NUnit.Framework;

namespace APIGateway.Mgmt.Core.Tests.Swagger.Aws
{
    [TestFixture]
    public class ApiGatewayDeploymentProviderTest
    {
        private ApiGatewayDeploymentProvider underTest;
        private Mock<IAmazonAPIGateway> gatewayMock;
        private string apiId = "pwfy7quvxh";

        public ApiGatewayDeploymentProviderTest()
        {
            gatewayMock = new Mock<IAmazonAPIGateway>();
            underTest = new ApiGatewayDeploymentProvider(gatewayMock.Object);
        }

        [Test]
        public void CreateDeploymentTest()
        {
            var config = TestHelper.Import<DeploymentDocument>("deployment.json");
            
            gatewayMock.Setup(x => x.PutIntegration(It.IsAny<PutIntegrationRequest>()))
                .Returns(new PutIntegrationResponse());

            underTest.CreateDeployment(apiId, config);

            gatewayMock.Verify(x => x.CreateDeployment(It.IsAny<CreateDeploymentRequest>()), Times.Once);
            gatewayMock.Verify(x => x.UpdateStage(It.IsAny<UpdateStageRequest>()), Times.Once);
        }

        [Test]
        public void CreateDomainTest()
        {
            var config = TestHelper.Import<DeploymentDocument>("deployment.json");

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
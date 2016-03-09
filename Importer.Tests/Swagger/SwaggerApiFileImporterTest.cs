using Importer.Swagger;
using Importer.Swagger.Impl;
using Moq;
using NUnit.Framework;

namespace Importer.Tests.Swagger
{
    [TestFixture]
    public class SwaggerApiFileImporterTest
    {
        private SwaggerApiFileImporter underTest;
        private Mock<ISwaggerApiImporter> mockImporter;
        private string apiId = "pwfy7quvxh";

        public SwaggerApiFileImporterTest()
        {
            mockImporter = new Mock<ISwaggerApiImporter>();
            underTest = new SwaggerApiFileImporter(mockImporter.Object);
        }

        [Test]
        public void ImportApiTest()
        {
            mockImporter.Setup(x => x.CreateApi(It.IsAny<SwaggerDocument>(), It.IsAny<string>())).Returns(apiId);
            var result = underTest.ImportApi(TestHelper.GetResourceFilePath("basic.json"));

            Assert.That(apiId, Is.EqualTo(result));
        }

        [Test]
        public void UpdateApiTest()
        {
            mockImporter.Verify(x => x.UpdateApi(It.IsAny<string>(), It.IsAny<SwaggerDocument>()), Times.AtMostOnce);
            underTest.UpdateApi(apiId, TestHelper.GetResourceFilePath("basic.json"));
        }

        [Test]
        public void DeleteApiTest()
        {
            mockImporter.Verify(x => x.DeleteApi(It.IsAny<string>()), Times.AtMostOnce);
            underTest.DeleteApi(apiId);
        }

        [Test]
        public void DeployApiTest()
        {
            mockImporter.Verify(x => x.Deploy(It.IsAny<string>(), It.IsAny<DeploymentConfig>()), Times.AtMostOnce);
            underTest.Deploy(apiId, TestHelper.GetResourceFilePath("deployment.json"));
        }

        [Test]
        public void ProvisionApiKeyTest()
        {
            var apiKey = "b097ed2dcea64b21bb38a0f2a26994cd";

            mockImporter.Setup(x => x.ProvisionApiKey(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(apiKey);
            mockImporter.Verify(x => x.ProvisionApiKey(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.AtMostOnce);
            var result = underTest.ProvisionApiKey(apiId, "api-key-name", "stage-name");

            Assert.That(apiKey, Is.EqualTo(result));
        }
    }
}

using System.IO;
using System.Reflection;
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
            var result = underTest.ImportApi(GetFilePath("basic.json"));

            Assert.That(apiId, Is.EqualTo(result));
        }

        [Test]
        public void UpdateApiTest()
        {
            mockImporter.Verify(x => x.UpdateApi(It.IsAny<string>(), It.IsAny<SwaggerDocument>()), Times.AtMostOnce);
            underTest.UpdateApi(apiId, GetFilePath("basic.json"));
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
            underTest.Deploy(apiId, GetFilePath("deployment.json"));
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

        /// <summary>
        /// Need to disable "| Options | Tools | Unit Testing | Shadow-copy assemblies being tested" to run unit tests from bin folder so the test file are picked-up.
        /// ReSharper https://www.jetbrains.com/resharper/help/Reference__Options__Tools__Unit_Testing.html
        /// </summary>
        private string GetFilePath(string swaggerFileName)
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var directory = new FileInfo(location).Directory;
            return Path.Combine(directory.FullName, $@"Resources\{swaggerFileName}");
        }
    }
}

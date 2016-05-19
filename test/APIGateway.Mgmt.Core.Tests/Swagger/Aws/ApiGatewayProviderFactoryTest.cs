using APIGateway.Management;
using APIGateway.Management.Impl;
using NUnit.Framework;

namespace APIGateway.Mgmt.Core.Tests.Swagger.Aws
{
    [TestFixture]
    public class ApiGatewayProviderFactoryTest
    {
        private ApiGatewayProviderFactory underTest;

        public ApiGatewayProviderFactoryTest()
        {
            underTest = new ApiGatewayProviderFactory();
        }

        [Test]
        public void CreateImporterTesttWithDefaultVersionTest()
        {
            var importer = underTest.Create();
            Assert.That(importer.GetType(), Is.EqualTo(typeof(ApiGatewayProvider)));
        }

        [Test]
        public void CreatImporterTestWithSepcifiedVersionTest()
        {
            var importer = underTest.Create();
            Assert.That(importer.GetType(), Is.EqualTo(typeof(ApiGatewayProvider)));
        }
    }
}

using System;
using Importer.Swagger.Aws.Impl;
using NUnit.Framework;

namespace Importer.Tests.Swagger.Aws
{
    [TestFixture]
    public class ApiGatewaySdkSwaggerApiImporterFactoryTest
    {
        private ApiGatewaySdkSwaggerApiImporterFactory underTest;

        public ApiGatewaySdkSwaggerApiImporterFactoryTest()
        {
            underTest = new ApiGatewaySdkSwaggerApiImporterFactory();
        }

        [Test]
        public void CreateImporterTesttWithDefaultVersionTest()
        {
            var importer = underTest.Create();
            Assert.That(importer.GetType(), Is.EqualTo(typeof(ApiGatewaySdkSwaggerApiImporter)));
        }

        [Test]
        public void CreatImporterTestWithSepcifiedVersionTest()
        {
            var importer = underTest.Create("v2.0");
            Assert.That(importer.GetType(), Is.EqualTo(typeof(ApiGatewaySdkSwaggerApiImporter)));
        }

        [Test]
        public void CreateImporterTestWithSepcifiedVersionTest()
        {
            Assert.Throws<NotSupportedException>(() => underTest.Create("v2.1"));
        }
    }
}

using System.Text;
using APIGateway.Management.Impl;
using APIGateway.Swagger;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace APIGateway.Mgmt.Core.Tests.Impl
{
    [TestFixture]
    public class SchemaTransformerTest
    {
        [Test]
        public void ComplexModelTest()
        {
            var models = Encoding.UTF8.GetString(SchemaResource.defintions);
            var modelSchema = Encoding.UTF8.GetString(SchemaResource.complex_schema);

            var schemaString = new SchemaTransformer().Flatten(modelSchema, models);
            var schema = Deserialize(schemaString);

            Assert.That(schema.Properties.Count, Is.EqualTo(16));
        }

        [Test]
        public void SimpleModelTest()
        {
            var models = Encoding.UTF8.GetString(SchemaResource.defintions);
            var modelSchema = Encoding.UTF8.GetString(SchemaResource.simple_schema);

            var schemaString = new SchemaTransformer().Flatten(modelSchema, models);
            var schema = Deserialize(schemaString);

            Assert.That(schema.Properties.Count, Is.EqualTo(3));
        }

        private Schema Deserialize(string schema)
        {
            var settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };

            return JsonConvert.DeserializeObject<Schema>(schema, settings);
        }
    }
}

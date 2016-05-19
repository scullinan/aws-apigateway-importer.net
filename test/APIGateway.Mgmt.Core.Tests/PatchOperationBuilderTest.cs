using APIGateway.Management.Impl;
using NUnit.Framework;

namespace APIGateway.Mgmt.Core.Tests
{
    [TestFixture]
    public class PatchOperationBuilderTest
    {
        [Test]
        public void AddOperationTest()
        {
            var builder = PatchOperationBuilder.With();

            var operations = builder
                .Operation(Operations.Add, "/path1", "value1")
                .Operation(Operations.Move, "/path2", "value2")
                .ToList();

            Assert.That(operations.Count, Is.EqualTo(2));
        }
    }
}
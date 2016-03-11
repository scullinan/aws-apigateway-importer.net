using System.IO;
using System.Reflection;
using Amazon.APIGateway.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Importer.Tests
{
    /// <summary>
    /// Need to disable "| Options | Tools | Unit Testing | Shadow-copy assemblies being tested" to run unit tests from bin folder so the test file are picked-up.
    /// ReSharper https://www.jetbrains.com/resharper/help/Reference__Options__Tools__Unit_Testing.html
    /// </summary>
    public static class TestHelper
    {
        public static string GetResourceFilePath(string swaggerFileName)
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var directory = new FileInfo(location).Directory;
            return Path.Combine(directory.FullName, $@"Resources\{swaggerFileName}");
        }

        public static T Import<T>(string swaggerFileName)
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var directory = new FileInfo(location).Directory;
            var path = Path.Combine(directory.FullName, $@"Resources\{swaggerFileName}");

            var serializer = new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver(), NullValueHandling = NullValueHandling.Ignore };

            var sr = new StreamReader(path);
            return serializer.Deserialize<T>(new JsonTextReader(sr));
        }

        public static Resource GetRootResource()
        {
            return new Resource() { Id = "id123", Path = "/" };
        }
    }
}

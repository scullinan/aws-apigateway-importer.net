using System.IO;
using System.Reflection;

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
    }
}

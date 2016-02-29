using System.Collections.Generic;
using CommandLine;

namespace AWS.APIGateway
{
    public class Options
    {
        [Option('u', "update", HelpText = "API ID to import swagger into an existing API")]
        public string ApiId { get; set; }

        [Option("delete", HelpText = "API ID to delete")]
        public string DeleteApiId { get; set; }

        [Option('c', "create", HelpText = "Create a new API")]
        public bool CreateNew { get; set; }

        [Option("files", HelpText = "Path to API definition file to import")]
        public IEnumerable<string> Files { get; set; }

        [Option('d', "deploy", HelpText = "Stage used to deploy the API (optional)")]
        public string DeploymentLabel { get; set; }

        [Option('t', "test", HelpText = "Create a new API (create only)")]
        public bool Cleanup { get; set; }

        [Option('r', "region", HelpText = "Create a new API (optional)")]
        public string Region { get; set; }

        [Option("help")]
        public bool Help { get; set; }
    }
}
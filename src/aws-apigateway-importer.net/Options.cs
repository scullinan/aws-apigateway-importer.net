using System.Collections.Generic;
using CommandLine;

namespace AWS.APIGateway
{
    public class Options
    {
        [Option('c', "create", HelpText = "Create a new API")]
        public IEnumerable<string> CreateFiles { get; set; }

        [Option('u', "update", HelpText = "API ID to import swagger into an existing API")]
        public string UpdateApiId { get; set; }

        [Option('f', "files", HelpText = "Create a new API")]
        public IEnumerable<string> Files { get; set; }

        [Option('d', "delete", HelpText = "API ID to delete")]
        public string DeleteApiId { get; set; }

        [Option('u', "deploy", HelpText = "Stage used to deploy the API (optional)")]
        public string DeploymentConfig { get; set; }

        [Option('t', "test", HelpText = "Create a new API (create only)")]
        public bool Cleanup { get; set; }

        [Option('r', "region", HelpText = "Create a new API (optional)")]
        public string Region { get; set; }
    }
}
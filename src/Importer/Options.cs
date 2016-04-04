using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CommandLine;

namespace Importer
{
    [ExcludeFromCodeCoverage]
    public class Options
    {
        [Option('c', "create", HelpText = "Create a new API")]
        public IList<string> CreateOption { get; set; }

        [Option('u', "update", HelpText = "import swagger into an existing API")]
        public IList<string> UpdateOption { get; set; }

        [Option('p', "merge", HelpText = " merge swagger into an existing API")]
        public IList<string> MergeOption { get; set; }

        [Option('d', "delete", HelpText = "API ID to delete")]
        public string DeleteApiId { get; set; }

        [Option('u', "deploy", HelpText = "Stage used to deploy the API (optional)")]
        public string DeploymentConfig { get; set; }

        [Option('t', "test", HelpText = "Create a new API (create only)")]
        public bool Cleanup { get; set; }

        [Option("key", HelpText = "Provision a API Key in a stage (optional)")]
        public IList<string> ProvisionConfig { get; set; }

        public bool Create => CreateOption.Any();

        public string UpdateApiId => UpdateOption.Count > 0 ? UpdateOption[0] : null;

        public string MergeApiId => MergeOption.Count > 0 ? MergeOption[0] : null;

        public IEnumerable<string> Files
        {
            get
            {
                if (CreateOption.Any())
                    return CreateOption;

                if (UpdateOption.Any())
                    return UpdateOption.Count > 1 ? UpdateOption.Skip(1) : Enumerable.Empty<string>();

                if (MergeOption.Any())
                    return MergeOption.Count > 1 ? MergeOption.Skip(1) : Enumerable.Empty<string>();

                return Enumerable.Empty<string>();
            }
        }
    }
}
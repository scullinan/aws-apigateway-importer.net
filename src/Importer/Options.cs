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

        [Option('u', "update", HelpText = "API ID to import swagger into an existing API")]
        public IList<string> UpdateOption { get; set; }

        [Option('p', "patch", HelpText = "API ID to import swagger into an existing API")]
        public IList<string> PathOption { get; set; }

        [Option('d', "delete", HelpText = "API ID to delete")]
        public string DeleteApiId { get; set; }

        [Option('u', "deploy", HelpText = "Stage used to deploy the API (optional)")]
        public string DeploymentConfig { get; set; }

        [Option('t', "test", HelpText = "Create a new API (create only)")]
        public bool Cleanup { get; set; }

        [Option('r', "region", HelpText = "Create a new API (optional)")]
        public string Region { get; set; }

        [Option("key", HelpText = "Provision a API Key in a stage (optional)")]
        public IList<string> ProvisionConfig { get; set; }

        public bool Create
        {
            get { return CreateOption.Any(); }
        }

        public string UpdateApiId
        {
            get
            {
                return UpdateOption.Count > 0 ? UpdateOption[0] : null;
            }
        }

        public string PatchApiId
        {
            get
            {
                return PathOption.Count > 0 ? PathOption[0] : null;
            }
        }

        public IEnumerable<string> Files
        {
            get
            {
                if (CreateOption.Any())
                    return CreateOption;

                if (UpdateOption.Any())
                    return UpdateOption.Count > 1 ? UpdateOption.Skip(1) : Enumerable.Empty<string>();

                if (PathOption.Any())
                    return PathOption.Count > 1 ? PathOption.Skip(1) : Enumerable.Empty<string>();

                return Enumerable.Empty<string>();
            }
        }
    }
}
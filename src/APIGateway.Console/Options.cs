using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using APIGateway.Management.Impl;
using CommandLine;

namespace APIGateway.Console
{
    [ExcludeFromCodeCoverage]
    public class Options
    {
        [Option('c', "create", HelpText = "Create a new API")]
        public IList<string> CreateOption { get; set; }

        [Option('u', "update", HelpText = "import swagger into an existing API")]
        public IList<string> UpdateOption { get; set; }

        [Option('m', "merge", HelpText = " merge swagger into an existing API")]
        public IList<string> MergeOption { get; set; }

        [Option('w', "wipe", HelpText = "API Id to wipe")]
        public string WipeApiId { get; set; }

        [Option('d', "delete", HelpText = "API Id to delete")]
        public string DeleteApiId { get; set; }

        [Option('u', "deploy", HelpText = "Stage used to deploy the API (optional)")]
        public string DeploymentConfig { get; set; }

        [Option('t', "test", HelpText = "Create a new API (create only)")]
        public bool Cleanup { get; set; }

        [Option("apikeys", HelpText = "manageAPI keys in a stage (optional)")]
        public IList<string> ApiKeyOptions { get; set; }

        [Option('l', "list", HelpText = "List APIs, API Keys & Operations, supported options apis|keys|ops")]
        public IList<string> ListOption { get; set; }

        [Option('e', "export", HelpText = "Export")]
        public IList<string> ExportOption { get; set; }

        [Option("combine", HelpText = "Combine swagger files")]
        public IList<string> CombineOption { get; set; }

        public bool Create => CreateOption.Any();

        public string UpdateApiId => UpdateOption.Count > 0 ? UpdateOption[0] : null;

        public string MergeApiId => MergeOption.Count > 0 ? MergeOption[0] : null;

        public string ExportApiId => ExportOption.Count > 0 ? ExportOption[0] : null;

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

        public string ListCommand => ListOption.Any() ? ValidateListCommand(ListOption[0]) : string.Empty;

        public string ApiKeyCommand => ApiKeyOptions.Any() ? ValidateApiKeyCommand(ApiKeyOptions[0]) : string.Empty;

        private string ValidateListCommand(string cmd)
        {
            switch (cmd)
            {
                case ListCommands.Apis:
                case ListCommands.Keys:
                case ListCommands.Ops:
                    return cmd;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cmd)); 
            }
        }

        private string ValidateApiKeyCommand(string cmd)
        {
            switch (cmd)
            {
                case ApiKeyCommands.Create:
                case ApiKeyCommands.Delete:
                    return cmd;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cmd));
            }
        }
    }
}
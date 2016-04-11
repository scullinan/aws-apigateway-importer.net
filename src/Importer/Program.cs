using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Autofac;
using CommandLine;
using Importer.Swagger;
using log4net;
using log4net.Config;

namespace Importer
{
    [ExcludeFromCodeCoverage]
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        private static IContainer Container { get; set; }

        static void Main(string[] args)
        {
            BasicConfigurator.Configure();
            var builder = new ContainerBuilder();
            Container = AutofacConfig.Configure(builder);

            Options options = null;
            var result = Parser.Default.ParseArguments<Options>(args);

            result.MapResult(opt => {
                options = opt;

                if (options.UpdateApiId == null && !options.Files.Any())
                    return 1;

                if (options.Cleanup && options.UpdateApiId != null)
                {
                    log.Error("Test mode is not supported when updating an API");
                    return 1;
                }

                if(options.Files.Any())
                { 
                    var fn = options.Files.First();
                    if (!File.Exists(fn))
                    {
                        log.ErrorFormat("Could not load file '{0}'", fn);
                        return 1;
                    }
                }

                return 0;
            },
            errors => {
                log.Error(errors);
                return 1;
            });

            var fileName = options.Files != null ? options.Files.FirstOrDefault() : string.Empty;
            Handle(options, fileName);
        }

        private static void Handle(Options options, string fileName)
        {
            var importer = Container.Resolve<ISwaggerApiFileImporter>();

            if (options.Create)
            {
                var apiId = importer.ImportApi(fileName);

                if (options.Cleanup)
                    importer.DeleteApi(apiId);
            }
            else if (!string.IsNullOrEmpty(options.UpdateApiId) && options.Files.Any())
            {
                importer.UpdateApi(options.UpdateApiId, fileName);
            }
            else if (!string.IsNullOrEmpty(options.MergeApiId) && options.Files.Any())
            {
                importer.MergeApi(options.MergeApiId, fileName);
            }
            else if (!string.IsNullOrEmpty(options.DeleteApiId))
            {
                importer.DeleteApi(options.DeleteApiId);
            }

            else if (!string.IsNullOrEmpty(options.WipeApiId))
            {
                importer.WipeApi(options.WipeApiId);
            }

            if (!string.IsNullOrEmpty(options.UpdateApiId) && !string.IsNullOrEmpty(options.DeploymentConfig))
            {
                importer.Deploy(options.UpdateApiId, options.DeploymentConfig);
            }

            if (!string.IsNullOrEmpty(options.ApiKeyCommand))
            {
                switch (options.ApiKeyCommand)
                {
                    case ApiKeyCommands.Create:
                    {
                        var apiKey = importer.ProvisionApiKey(options.UpdateApiId, options.ApiKeyOptions[1],
                            options.ApiKeyOptions[2]);
                        log.InfoFormat("ApiKey {0} created for API id {1}", apiKey, options.UpdateApiId);
                        break;
                    }
                    case ApiKeyCommands.Delete:
                    {
                        importer.DeleteApiKey(options.ApiKeyOptions[1]);
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(options.ListCommand))
            {
                switch (options.ListCommand)
                {
                    case ListCommands.Apis:
                    {
                        foreach (var item in importer.ListApis())
                        {
                            Console.WriteLine("Id : {0} : {1}", item.Key, item.Value);
                        }
                        break;
                    }
                    case ListCommands.Keys:
                    {
                        foreach (var item in importer.ListKeys())
                        {
                            Console.WriteLine("Id : {0} : {1}", item.Key, item.Value);
                        }
                        break;
                    }
                }
            }
        }
    }
}

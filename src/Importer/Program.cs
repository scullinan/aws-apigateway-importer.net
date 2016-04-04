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

            var fileName = options.Files.FirstOrDefault();
            ImportSwagger(options, fileName);

            Console.ReadLine();
        }

        private static void ImportSwagger(Options options, string fileName)
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
            else if (!string.IsNullOrEmpty(options.PatchApiId) && options.Files.Any())
            {
                importer.PatchApi(options.PatchApiId, fileName);
            }
            else if (!string.IsNullOrEmpty(options.DeleteApiId))
            {
                importer.DeleteApi(options.DeleteApiId);
            }

            if (!string.IsNullOrEmpty(options.DeploymentConfig) && !string.IsNullOrEmpty(options.UpdateApiId))
            {
                importer.Deploy(options.UpdateApiId, options.DeploymentConfig);
            }

            if (options.ProvisionConfig.Any())
            {
                var apiKey = importer.ProvisionApiKey(options.UpdateApiId, options.ProvisionConfig[0], options.ProvisionConfig[1]);
                log.InfoFormat("ApiKey {0} created for API id {1}", apiKey, options.UpdateApiId);
            }
        }
    }
}

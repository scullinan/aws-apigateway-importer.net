using System.IO;
using System.Linq;
using Autofac;
using CommandLine;
using Importer.Aws.Impl;
using Importer.Impl;
using Importer.Swagger;
using Importer.Swagger.Impl;
using log4net;
using log4net.Config;

namespace Importer
{
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
        }

        private static void ImportSwagger(Options options, string fileName)
        {
            var importer = Container.Resolve<ISwaggerApiFileImporter>();

            var apiId = string.Empty; 

            if (options.Create)
            {
                apiId = importer.ImportApi(fileName);

                if (options.Cleanup)
                    importer.DeleteApi(apiId);
                
            }
            else if (!string.IsNullOrEmpty(options.DeleteApiId))
            {
                apiId = options.DeleteApiId;
                importer.DeleteApi(options.DeleteApiId);
            }
            else if (!string.IsNullOrEmpty(options.UpdateApiId) && options.Files.Any())
            {
                apiId = options.UpdateApiId;
                importer.UpdateApi(options.UpdateApiId, fileName);
            }

            if (!string.IsNullOrEmpty(options.DeploymentConfig) && !string.IsNullOrEmpty(apiId))
            {
                importer.Deploy(apiId, options.DeploymentConfig);
            }

            if (options.ProvisionConfig.Any())
            {
                var apiKey = importer.ProvisionApiKey(options.UpdateApiId, options.ProvisionConfig[0], options.ProvisionConfig[1]);
                log.InfoFormat("ApiKey {0} created for API id {1}", apiKey, options.UpdateApiId);
            }
        }
    }
}

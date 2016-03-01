using System.IO;
using System.Linq;
using AWS.APIGateway.Impl;
using CommandLine;
using log4net;
using log4net.Config;

namespace AWS.APIGateway
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            BasicConfigurator.Configure();
            Options options = null;
            var result = Parser.Default.ParseArguments<Options>(args);

            var exitCode = result.MapResult(opt => {
                options = opt;

                if (options.UpdateApiId == null && !options.CreateFiles.Any())
                    return 1;

                if (options.Cleanup && options.UpdateApiId != null)
                {
                    log.Error("Test mode is not supported when updating an API");
                    return 1;
                }

                var fn = options.Files.First();
                if (!File.Exists(fn))
                {
                    log.ErrorFormat("Could not load file '{0}'", fn);
                    return 1;
                }

                return 0;
            },
            errors => {
                log.Error(errors);
                return 1;
            });

            var fileName = options.CreateFiles.FirstOrDefault();
            ImportSwagger(options, fileName);
        }

        private static void ImportSwagger(Options options, string fileName)
        {
            ISwaggerApiFileImporter importer = new ApiGatewaySwaggerApiFileImporter(new ApiGatewaySdkSwaggerApiImporter()); //ToDo

            if (options.CreateFiles.Any())
            {
                var apiId = importer.ImportApi(fileName);

                log.InfoFormat("Api with ID {0} created", apiId);

                if (options.Cleanup)
                {
                    importer.DeleteApi(apiId);
                }
            }
            else if (!string.IsNullOrEmpty(options.DeleteApiId))
            {
                importer.DeleteApi(options.DeleteApiId);
            }

            //ToDo:Update
            
            if (!string.IsNullOrEmpty(options.DeploymentConfig))
            {
                importer.Deploy(options.UpdateApiId, options.DeploymentConfig);
            }
        }
    }
}

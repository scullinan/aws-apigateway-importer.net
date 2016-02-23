using System;
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
        static void Main(string[] args)
        {
            ILog log = LogManager.GetLogger(typeof (Program));

            BasicConfigurator.Configure();
            Options options = null;
            var result = Parser.Default.ParseArguments<Options>(args);

            var exitCode = result.MapResult(opt => {
                options = opt;

                if ((options.ApiId == null && !options.CreateNew) || options.Files == null || !options.Files.Any())
                    return 1;

                if (options.Cleanup && options.ApiId != null)
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

            var fileName = options.Files.FirstOrDefault();

            if (Path.GetExtension(fileName).Equals("raml"))
            {
                ImportRaml(options, fileName);
            }
            else
            {
                ImportSwagger(options, fileName);
            }

            Console.ReadLine();
        }

        private static void ImportRaml(Options options, string fileName)
        {
            throw new NotImplementedException("RAML import has not been implemented");
        }

        private static void ImportSwagger(Options options, string fileName)
        {
            ISwaggerApiFileImporter importer = new ApiGatewaySwaggerApiFileImporter(new ApiGatewaySdkSwaggerApiImporter()); //ToDo

            if (options.CreateNew)
            {
                var apiId = importer.ImportApi(fileName).Result;

                if (options.Cleanup)
                {
                    importer.DeleteApi(apiId);
                }
            }
            else
            {
                importer.UpdateApi(options.ApiId, fileName);
            }

            if (!string.IsNullOrEmpty(options.DeploymentLabel))
            {
                importer.Deploy(options.ApiId, options.DeploymentLabel);
            }
        }
    }
}

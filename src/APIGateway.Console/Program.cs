using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using APIGateway.Management;
using APIGateway.Management.Impl;
using APIGateway.Swagger;
using Autofac;
using CommandLine;
using log4net;
using log4net.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace APIGateway.Console
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

                if (options.Files.Any())
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
            var importer = Container.Resolve<IApiGatewayProvider>();
            SwaggerDocument swagger = null;

            if (!string.IsNullOrEmpty(fileName))
                swagger = Import<SwaggerDocument>(fileName);

            if (options.Create)
            {
                var name = Path.GetFileName(fileName);           
                var apiId = importer.Create(name, swagger);

                if (options.Cleanup)
                    importer.Delete(apiId);
            }
            else if (!string.IsNullOrEmpty(options.UpdateApiId) && options.Files.Any())
            {
                importer.Update(options.UpdateApiId, swagger);
            }
            else if (!string.IsNullOrEmpty(options.MergeApiId) && options.Files.Any())
            {
                importer.Merge(options.MergeApiId, swagger);
            }
            else if (!string.IsNullOrEmpty(options.DeleteApiId))
            {
                importer.Delete(options.DeleteApiId);
            }

            else if (!string.IsNullOrEmpty(options.WipeApiId))
            {
                importer.Destory(options.WipeApiId);
            }

            if (!string.IsNullOrEmpty(options.UpdateApiId) && !string.IsNullOrEmpty(options.DeploymentConfig))
            {
                var config = Import<DeploymentDocument>(options.DeploymentConfig);
                importer.Deploy(options.UpdateApiId, config);
            }

            if (!string.IsNullOrEmpty(options.ApiKeyCommand))
            {
                switch (options.ApiKeyCommand)
                {
                    case ApiKeyCommands.Create:
                        {
                            var apiKey = importer.CreateApiKey(options.UpdateApiId, options.ApiKeyOptions[1],
                                options.ApiKeyOptions[2]);
                            log.InfoFormat("ApiKey {0} created for API id {1}", apiKey, options.UpdateApiId);
                            break;
                        }
                    case ApiKeyCommands.Delete:
                        {
                            importer.DeleteApiKey(options.ApiKeyOptions[1]);
                            break;
                        }
                    case ApiKeyCommands.Clear:
                        {
                            importer.ClearApiKeys();
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
                                System.Console.WriteLine("Id : {0} : {1}", item.Key, item.Value);
                            }
                            break;
                        }
                    case ListCommands.Keys:
                        {
                            foreach (var item in importer.ListKeys())
                            {
                                System.Console.WriteLine("Id : {0} : {1}", item.Key, item.Value);
                            }
                            break;
                        }
                    case ListCommands.Ops:
                        {
                            if (options.ListOption.Count > 1)
                                foreach (var item in importer.ListOperations(options.ListOption[1]))
                                {
                                    System.Console.WriteLine(item);
                                }
                            break;
                        }
                }
            }

            if (!string.IsNullOrEmpty(options.ExportApiId))
            {
                var json = importer.Export(options.ExportApiId, options.ExportOption[1]);
                File.WriteAllText(@"output.json", json);
            }

            if (options.CombineOption.Any() && options.CombineOption.Count > 2)
            {
                var files = options.CombineOption.Skip(1);
                var documents = files.Select(Import<SwaggerDocument>).ToList();

                var json = importer.Combine(documents);
                File.WriteAllText(options.CombineOption[0], Export(json));
            }
        }

        private static T Import<T>(string filePath)
        {
            var serializer = new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver(), NullValueHandling = NullValueHandling.Ignore };

            var sr = new StreamReader(filePath);
            return serializer.Deserialize<T>(new JsonTextReader(sr));
        }

        private static string Export<T>(T document)
        {
            return JsonConvert.SerializeObject(document, Formatting.Indented, new JsonSerializerSettings()
            {
                ContractResolver = new SwaggerContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            });
        }
    }
}

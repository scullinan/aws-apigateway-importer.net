using System.Diagnostics.CodeAnalysis;
using Autofac;
using Importer.Swagger;
using Importer.Swagger.Aws.Impl;
using Importer.Swagger.Impl;

namespace Importer
{
    [ExcludeFromCodeCoverage]
    public class AutofacConfig
    {
        public static IContainer Configure(ContainerBuilder builder)
        {
            builder.RegisterType<ApiGatewaySdkSwaggerApiImporterFactory>()
                .As<ISwaggerApiImporterFactory>();

            builder.Register(x => {
                var factory = x.Resolve<ISwaggerApiImporterFactory>();
                var importer = factory.Create();
                return new SwaggerApiFileImporter(importer);
            })
            .As<ISwaggerApiFileImporter>();

            return builder.Build();
        }
    }
}

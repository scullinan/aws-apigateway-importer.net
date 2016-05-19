using System.Diagnostics.CodeAnalysis;
using APIGateway.Management;
using APIGateway.Management.Impl;
using Autofac;

namespace APIGateway.Console
{
    [ExcludeFromCodeCoverage]
    public class AutofacConfig
    {
        public static IContainer Configure(ContainerBuilder builder)
        {
            builder.RegisterType<ApiGatewayProviderFactory>()
                .As<IApiGatewayProviderFactory>();

            builder.Register(x =>
            {
                var factory = x.Resolve<IApiGatewayProviderFactory>();
                var provider = factory.Create();
                return provider;
            }).As<IApiGatewayProvider>();

            return builder.Build();
        }
    }
}

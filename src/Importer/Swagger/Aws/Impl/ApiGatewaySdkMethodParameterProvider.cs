using System.Collections.Generic;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using log4net;

namespace Importer.Swagger.Aws.Impl
{
    public class ApiGatewaySdkMethodParameterProvider : IApiGatewaySdkMethodParameterProvider
    {
        protected ILog Log = LogManager.GetLogger(typeof(ApiGatewaySdkDeploymentProvider));
        private readonly IAmazonAPIGateway gateway;

        public ApiGatewaySdkMethodParameterProvider(IAmazonAPIGateway gateway)
        {
            this.gateway = gateway;
        }

        public void CreateMethodParameters(RestApi api, Resource resource, Method method, IList<Parameter> parameters)
        {
            parameters.ForEach(p =>
            {
                if (!p.In.Equals("body"))
                {
                    if (!string.IsNullOrEmpty(GetParameterLocation(p)))
                    {
                        string expression = CreateRequestParameterExpression(p);

                        Log.InfoFormat("Creating method parameter for api {0} and method {1} with name {2}", api.Id, method.HttpMethod, expression);

                        var operations = PatchOperationBuilder.With()
                            .Operation(Operations.Add, "/requestParameters/" + expression, p.Required.ToString())
                            .ToList();

                        var request = new UpdateMethodRequest
                        {
                            RestApiId = api.Id,
                            ResourceId = resource.Id,
                            HttpMethod = method.HttpMethod,
                            PatchOperations = operations
                        };

                        gateway.UpdateMethod(request);
                    }
                }
            });
        }

        public void UpdateMethodParameters(RestApi api, Resource resource, Method method, IList<Parameter> parameters)
        {
            if (method.RequestParameters != null)
            {
                foreach (var parameter in method.RequestParameters.Keys)
                {
                    var operations = PatchOperationBuilder.With()
                        .Operation(Operations.Remove, "/requestParameters/" + parameter)
                        .ToList();

                    gateway.UpdateMethod(new UpdateMethodRequest()
                    {
                        RestApiId = api.Id,
                        ResourceId = resource.Id,
                        HttpMethod = method.HttpMethod,
                        PatchOperations = operations
                    });
                }
            }

            CreateMethodParameters(api, resource, method, parameters);
        }

        private string GetParameterLocation(Parameter p)
        {
            switch (p.In)
            {
                case "path":
                    return "path";
                case "query":
                    return "querystring";
                case "header":
                    return "header";
                default:
                    Log.WarnFormat("Parameter type {0} not supported, skipping", p.In);
                    break;
            }
            return string.Empty;
        }

        private string CreateRequestParameterExpression(Parameter p)
        {
            string loc = GetParameterLocation(p);
            return string.Format("method.request.{0}.{1}", loc, p.Name);
        }
    }
}
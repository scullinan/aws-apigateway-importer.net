using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.APIGateway.Model;

namespace AWS.APIGateway.Impl
{
    public partial class ApiGatewaySdkSwaggerApiImporter
    {
        private async Task CreateMethodParameters(RestApi api, Resource resource, Method method, IList<Parameter> parameters)
        {
            parameters.ForEach(async p =>
            {
                if (!p.In.Equals("body"))
                {
                    if (!string.IsNullOrEmpty(GetParameterLocation(p)))
                    {
                        string expression = CreateRequestParameterExpression(p);

                        Log.InfoFormat("Creating method parameter for api {0} and method {1} with name {2}", api.Id, method.HttpMethod, expression);

                        var request = new UpdateMethodRequest {
                            RestApiId = api.Id,
                            ResourceId = resource.Id,
                            HttpMethod = method.HttpMethod,
                            PatchOperations = new List<PatchOperation>() {
                                CreateAddOperation("/requestParameters/" + expression, p.Required.ToString())
                            }
                        };

                        await Client.UpdateMethodAsync(request);
                    }
                }
            });
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
                    return"header";
                default:
                    Log.WarnFormat("Parameter type {0} not supported, skipping", p.In);
                    break;
            }
            return string.Empty;
        }

        private string CreateRequestParameterExpression(Parameter p)
        {
            string loc = GetParameterLocation(p);
            return string.Format("method.request.{0}.{1}", loc , p.Name);
        }

        public static PatchOperation CreateAddOperation(string path, string value)
        {
            var op = new PatchOperation
            {
                Op = "add",
                Path = path,
                Value = value
            };

            return op;
        }

    }
}

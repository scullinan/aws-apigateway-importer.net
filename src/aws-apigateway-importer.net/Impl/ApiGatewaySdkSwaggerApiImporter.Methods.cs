using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.APIGateway.Model;
using Newtonsoft.Json.Linq;

namespace AWS.APIGateway.Impl
{
    //Methods
    public partial class ApiGatewaySdkSwaggerApiImporter
    {
        private void CreateMethods(RestApi api, Resource resource, PathItem path, IList<string> apiProduces)
        {
            var ops = GetOperations(path);

            ops.ForEach(x => { 
                CreateMethod(api, resource, x.Key, x.Value, GetProducesContentType(apiProduces, x.Value.Produces));
                Log.InfoFormat("Creating method for api id {0} and resource id {1} with method {2}", api.Id, resource.Id, x.Key);
            });
        }

        private IDictionary<string, Operation> GetOperations(PathItem path)
        {
            IDictionary<string, Operation> ops = new Dictionary<string, Operation>();

            AddOp(ops, "get", path.Get);
            AddOp(ops, "post", path.Post);
            AddOp(ops, "put", path.Put);
            AddOp(ops, "delete", path.Delete);
            AddOp(ops, "options", path.Options);
            AddOp(ops, "patch", path.Patch);

            return ops;
        }

        private void AddOp(IDictionary<string, Operation> ops, string method, Operation operation)
        {
            if (operation != null)
            {
                ops[method] = operation;
            }
        }

        public void CreateMethod(RestApi api, Resource resource, string httpMethod, Operation op, string modelContentType)
        {
            var input = new PutMethodRequest
            {
                AuthorizationType = GetAuthorizationType(op),
                ApiKeyRequired = IsApiKeyRequired(op),
                RestApiId = api.Id,
                ResourceId = resource.Id
            };

            // set input model if present in body
            op.Parameters.Where(x => x.In.Equals("body")).ForEach(p => {
                //BodyParameter bodyParam = (BodyParameter)p;

                var inputModel = GetInputModel(p);

                input.RequestModels = new Dictionary<string, string>();
                
                // model already imported
                if (inputModel != null)
                {
                    ProcessedModels.Add(inputModel);

                    Log.InfoFormat("Found input model reference {0}", inputModel);
                    input.RequestModels[modelContentType] = inputModel;
                }
                else
                {
                    // create new model from nested schema
                    string modelName = GenerateModelName(p);
                    Log.InfoFormat("Creating new model referenced from parameter: {0}", modelName);

                    if (p.Schema == null)
                    {
                        throw new ArgumentException("Body parameter '{0}' + must have a schema defined", p.Name);
                    }

                    CreateModel(api, modelName, p.Schema, Swagger.Definitions, modelContentType);
                    input.RequestModels[modelContentType] = modelName;
                }
            });

            // create method
            input.HttpMethod = httpMethod.ToUpper();
            var result = Client.PutMethod(input);

            var method = new Method()
            {
                HttpMethod = result.HttpMethod,
                ApiKeyRequired = result.ApiKeyRequired,
                AuthorizationType = result.AuthorizationType,
                MethodIntegration = result.MethodIntegration,
                MethodResponses = result.MethodResponses,
                RequestModels = result.RequestModels,
                RequestParameters = result.RequestParameters
            };


            CreateMethodResponses(api, resource, method, modelContentType, op.Responses);
            CreateMethodParameters(api, resource, method, op.Parameters);
            CreateIntegration(api, resource, method, op.VendorExtensions);
        }

        private string GetAuthorizationType(Operation op)
        {
            var authType = "NONE";

            if (op.VendorExtensions != null && op.VendorExtensions.ContainsKey(EXTENSION_AUTH))
            {
                var vendorExtension = op.VendorExtensions[EXTENSION_AUTH] as JObject;

                var authExtension = vendorExtension?.ToObject<Dictionary<string, string>>();

                if (authExtension != null)
                {
                    authType = authExtension["type"].ToUpper();
                }
            }

            return authType;
        }

        private bool IsApiKeyRequired(Operation op)
        {
            var apiKeySecurityDefinition = default(KeyValuePair<string, SecurityScheme>);

            if (Swagger.SecurityDefinitions != null)
            {
                apiKeySecurityDefinition = Swagger.SecurityDefinitions.FirstOrDefault(x => x.Value.Type.Equals("apiKey"));
            }

            if (apiKeySecurityDefinition.Equals(default(KeyValuePair<string, SecurityScheme>)))
            {
                return false;
            }

            var securityDefinitionName = apiKeySecurityDefinition.Key;

            if (op.Security != null)
            {
                return op.Security.Any(x => x.ContainsKey(securityDefinitionName));
            }

            if (Swagger.Security != null)
            {
                return Swagger.Security.Any(x => x.ContainsKey(securityDefinitionName));
            }

            return false;
        }


        private string GetInputModel(Parameter p)
        {
            var model = p.Schema;

            if (p.Schema.Ref != null)
                return p.Schema.Ref;

            //Todo:What is this? based on input
            //if (model instanceof RefModel) {
            //    String modelName = ((RefModel)model).getSimpleRef();   // assumption: complex ref?
            //    return Optional.of(modelName);
            //}

            return string.Empty;
        }

        string GenerateModelName(Response response)
        {
            return GenerateModelName(response.Description);
        }

        private string GenerateModelName(Parameter param)
        {
            return GenerateModelName(param.Description);
        }

        private string GenerateModelName(string description)
        {
            if (string.IsNullOrEmpty(description))
            {
                Log.Warn("No description found for model, will generate a unique model name");
                return "model" + Guid.NewGuid().ToString().Substring(0, 8);
            }

            // note: generating model name based on sanitized description
            return description.Replace(GetModelNameSanitizeRegex(), "");
        }

        private string GetModelNameSanitizeRegex()
        {
            return "[^A-Za-z0-9]";
        }

        private Model GetModel(RestApi api, Response response)
        {

            string modelName;

            // if the response references a proper model, look for a model matching the model name
            if (response.Schema != null && response.Schema.Type.Equals("ref"))
            {
                modelName = response.Schema.Ref;
            }
            else
            {
                // if the response has an embedded schema, look for a model matching the generated name
                modelName = GenerateModelName(response);
            }

            try
            {
                var result = Client.GetModel(new GetModelRequest() { RestApiId = api.Id, ModelName = modelName });
                return new Model()
                {
                    Description = result.Description,
                    ContentType = result.ContentType,
                    
                };
            }
            catch (Exception ignored) { }

            return null;
        }
    }
}

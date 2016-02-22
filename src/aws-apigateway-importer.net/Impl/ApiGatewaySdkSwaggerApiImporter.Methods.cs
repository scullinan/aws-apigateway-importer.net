using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.APIGateway.Model;

namespace aws_apigateway_importer.net.Impl
{
    //Methods
    public partial class ApiGatewaySdkSwaggerApiImporter
    {
        private void CreateMethods(RestApi api, Resource resource, PathItem path, IList<string> apiProduces)
        {
            var ops = GetOperations(path);

            ops.ForEach(async x => { 
                await CreateMethod(api, resource, x.Key, x.Value, GetProducesContentType(apiProduces, x.Value.Produces));
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

        public async Task CreateMethod(RestApi api, Resource resource, string httpMethod, Operation op, string modelContentType)
        {
            var input = new PutMethodRequest
            {
                AuthorizationType = GetAuthorizationType(op),
                ApiKeyRequired = IsApiKeyRequired(op)
            };

            // set input model if present in body
            op.Parameters.Where(x => x.In.Equals("body")).ForEach(async p => {
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

                    await CreateModel(api, modelName, p.Schema, Swagger.Definitions, modelContentType);
                    input.RequestModels[modelContentType] = modelName;
                }
            });

            // create method
            input.HttpMethod = httpMethod.ToUpper();
            var method = await Client.PutMethodAsync(input);

            await CreateMethodResponses(api, method, modelContentType, op.Responses);
            CreateMethodParameters(api, method, op.Parameters);
            CreateIntegration(method, op.VendorExtensions);
        }

        private string GetAuthorizationType(Operation op)
        {
            var authType = "NONE";
            if (op.VendorExtensions != null)
            {
                var authExtension = (IDictionary<string, string>)op.VendorExtensions[(EXTENSION_AUTH];

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

            //Todo What is SecurityRequirements in Swagger
            //if (Swagger.getSecurityRequirement != null)
            //{
            //    return Swagger.getSecurityRequirement().stream().anyMatch(s->s.getName().equals(securityDefinitionName));
            //}
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

        private async Task CreateMethodResponses(RestApi api, PutMethodResponse method, string modelContentType, IDictionary<string, Response> responses)
        {
            if (responses == null)
            {
                return;
            }

            // add responses from swagger
            responses.ForEach(async x => {

                if (x.Key.Equals("default"))
                {
                    Log.Warn("Default response not supported, skipping");
                }
                else
                {
                    Log.InfoFormat("Creating method response for api {0} and method {1} and status {2}", api.Id, method.HttpMethod, x.Key);

                    var request = GetCreateResponseInput(api, modelContentType, x.Value);
                    request.StatusCode = x.Key;


                    await Client.PutMethodResponseAsync(request);
                }
            });
        }

        private PutMethodResponseRequest GetCreateResponseInput(RestApi api, String modelContentType, Response response)
        {

            PutMethodResponseRequest input = new PutMethodResponseRequest();

            // add response headers
            if (response.Headers != null)
            {
                input.ResponseParameters = new Dictionary<string, bool>();
                response.getHeaders().entrySet().forEach(
                        e->input.getResponseParameters().put("method.response.header." + e.getKey(), e.getValue().getRequired()));
            }

            // if the schema references an existing model, use that model for the response
            Optional<Model> modelOpt = getModel(api, response);
            if (modelOpt.isPresent())
            {
                input.setResponseModels(new HashMap<>());
                String modelName = modelOpt.get().getName();
                input.getResponseModels().put(modelContentType, modelName);
                this.processedModels.add(modelName);
                LOG.info("Found reference to existing model " + modelName);
            }
            else
            {
                // generate a model based on the schema if the model doesn't already exist
                if (response.getSchema() != null)
                {
                    String modelName = generateModelName(response);

                    LOG.info("Creating new model referenced from response: " + modelName);

                    createModel(api, modelName, response.getSchema(), modelContentType);

                    input.setResponseModels(new HashMap<>());
                    input.getResponseModels().put(modelContentType, modelName);
                }
            }

            return input;
        }
    }
}

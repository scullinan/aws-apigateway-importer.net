using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.APIGateway.Model;

namespace aws_apigateway_importer.net.Impl
{
    public partial class ApiGatewaySdkSwaggerApiImporter
    {
        private async Task CreateMethodResponses(RestApi api, Resource resource, Method method, string modelContentType, IDictionary<string, Response> responses)
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

                    var request = await GetCreateResponseInput(api, modelContentType, x.Value);
                    request.RestApiId = api.Id;
                    request.ResourceId = resource.Id;
                    request.StatusCode = x.Key;
                    request.HttpMethod = method.HttpMethod;
                    
                    await Client.PutMethodResponseAsync(request);
                }
            });
        }

        private async Task<PutMethodResponseRequest> GetCreateResponseInput(RestApi api, string modelContentType, Response response)
        {

            PutMethodResponseRequest input = new PutMethodResponseRequest();

            // add response headers
            if (response.Headers != null)
            {
                input.ResponseParameters = new Dictionary<string, bool>();
                response.Headers.ForEach(x =>
                {
                    input.ResponseParameters["method.response.header." + x.Key] = x.Value.Required;
                }); //ToDo Required?
            }

            // if the schema references an existing model, use that model for the response
            Model modelOpt = await GetModel(api, response);
            if (modelOpt != null)
            {
                input.ResponseModels = new Dictionary<string, string>();
                var modelName = modelOpt.Name;

                input.ResponseModels[modelContentType] = modelName;
                this.ProcessedModels.Add(modelName);
                Log.InfoFormat("Found reference to existing model {0}", modelName);
            }
            else
            {
                // generate a model based on the schema if the model doesn't already exist
                if (response.Schema != null)
                {
                    string modelName = GenerateModelName(response);

                    Log.InfoFormat("Creating new model referenced from response: {0}", modelName);

                    await CreateModel(api, modelName, response.Schema, modelContentType);

                    input.ResponseModels = new Dictionary<string, string> {[modelContentType] = modelName};
                }
            }

            return input;
        }
    }
}

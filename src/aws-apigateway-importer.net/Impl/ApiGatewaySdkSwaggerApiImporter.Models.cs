using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.APIGateway.Model;

namespace aws_apigateway_importer.net.Impl
{
    //Models
    public partial class ApiGatewaySdkSwaggerApiImporter
    {
        private async Task CreateModels(RestApi api, IDictionary<string, Schema> definitions, IList<string> produces)
        {
            if (definitions == null)
            {
                return;
            }

            foreach (var definition in definitions)
            {
                var modelName = definition.Key;
                var model = definition.Value;

                await CreateModel(api, modelName, model, definitions, GetProducesContentType(produces, Enumerable.Empty<string>()));
            }

        }

        private async Task CreateModel(RestApi api, string modelName, Schema model, IDictionary<string, Schema> definitions, string modelContentType)
        {
            if (modelName == null) throw new ArgumentNullException(nameof(modelName));
            log.InfoFormat("Creating model for api id {0} with name {1}", api.Id, modelName);

            await CreateModel(api, modelName, model.Description, GenerateSchema(model, modelName, definitions), modelContentType);
        }

        private async Task CreateModel(RestApi api, String modelName, Schema model, string modelContentType)
        {
            Log.InfoFormat("Creating model for api id {0} with name {1}", api.Id, modelName);

            await CreateModel(api, modelName, model.Description, GenerateSchema(model, modelName, Swagger.Definitions), modelContentType);
        }
    }
}

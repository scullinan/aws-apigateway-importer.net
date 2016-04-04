using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using log4net;

namespace Importer.Swagger.Aws.Impl
{
    public class ApiGatewaySdkModelProvider : IApiGatewaySdkModelProvider
    {
        private readonly HashSet<string> processedModels;
        private readonly IAmazonAPIGateway gateway;
        protected ILog Log = LogManager.GetLogger(typeof(ApiGatewaySdkModelProvider));

        public ApiGatewaySdkModelProvider(HashSet<string> processedModels, IAmazonAPIGateway gateway, IModelNameResolver modelNameResolver)
        {
            this.processedModels = processedModels;
            this.gateway = gateway;
            this.NameResolver = modelNameResolver;
        }

        public IModelNameResolver NameResolver { get; }

        public void CreateModels(RestApi api, SwaggerDocument swagger)
        {
            if (swagger.Definitions == null)
            {
                return;
            }

            foreach (var definition in swagger.Definitions)
            {
                var modelName = NameResolver.Sanitize(definition.Key); //Remove any special charcters
                var model = definition.Value;

                CreateModel(api, modelName, model, swagger.Definitions, SwaggerHelper.GetProducesContentType(swagger.Produces, Enumerable.Empty<string>()));
            }
        }

        public void CreateModel(RestApi api, string modelName, Schema model, IDictionary<string, Schema> definitions, string modelContentType)
        {
            if (modelName == null) throw new ArgumentNullException(nameof(modelName));
            Log.InfoFormat("Creating model for api id {0} with name {1}", api.Id, modelName);

            var schema = SwaggerHelper.GenerateSchema(model, modelName, definitions);

            if (schema == null) throw new ArgumentNullException(nameof(schema));
            processedModels.Add(modelName);

            var input = new CreateModelRequest
            {
                RestApiId = api.Id,
                Name = modelName,
                Description = model.Description,
                ContentType = modelContentType,
                Schema = schema
            };
            
            gateway.WaitAndRetry(x => x.CreateModel(input));
        }

        public void DeleteDefaultModels(RestApi api)
        {
            var models = gateway.BuildModelList(api.Id);
            models.ForEach(model =>  {
                Log.InfoFormat("Removing default model {0}", model.Name);

                gateway.WaitAndRetry(x => x.DeleteModel(new DeleteModelRequest() {
                    RestApiId = api.Id,
                    ModelName = model.Name
                }));
            });
        }

        public void UpdateModels(RestApi api, SwaggerDocument swagger)
        {
            if(swagger.Definitions == null)
                return;

            foreach (var definition in swagger.Definitions)
            {
                var modelName = NameResolver.Sanitize(definition.Key);
                var model = definition.Value;

                if (gateway.DoesModelExists(api.Id, modelName))
                {
                    UpdateModel(api, modelName, model, swagger.Definitions);       
                }
                else
                {
                    CreateModel(api, modelName, model, swagger.Definitions, SwaggerHelper.GetProducesContentType(swagger.Produces, Enumerable.Empty<string>()));
                }
            }
        }

        public void CleanupModels(RestApi api)
        {
            var existingModels = gateway.BuildModelList(api.Id);
            var modelsToDelete = existingModels.Where(x => !processedModels.Contains(x.Name));

            modelsToDelete.ForEach(x =>
            {
                Log.InfoFormat("Removing deleted model {0}", x.Name);
                gateway.WaitAndRetry(y => y.DeleteModel(new DeleteModelRequest()
                {
                    RestApiId = api.Id,
                    ModelName = x.Name
                }));
            });
        }

        private void UpdateModel(RestApi api, string modelName, Schema model, IDictionary<string, Schema> definitions)
        {
            Log.InfoFormat("Updating model for api id {0} with name {1}", api.Id, modelName);

            var schema = SwaggerHelper.GenerateSchema(model, modelName, definitions);

            if (schema == null) throw new ArgumentNullException(nameof(schema));
            processedModels.Add(modelName);

            var operations = PatchOperationBuilder.With()
                .Operation(Operations.Replace, "/schema", schema)
                .ToList();

            gateway.WaitAndRetry(x => x.UpdateModel(new UpdateModelRequest()
            {
                RestApiId = api.Id,
                ModelName = modelName,
                PatchOperations = operations
            }));
        }
    }
}
using System;
using System.Collections.Generic;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using Polly;

namespace Importer.Swagger
{
    public static class GatewayExtentions
    {
        public static bool DoesModelExists(this IAmazonAPIGateway gateway, string apiId, string modelName)
        {
            try
            {
                gateway.WaitAndRetry(x => x.GetModel(new GetModelRequest() {RestApiId = apiId, ModelName = modelName}));
            }
            catch (NotFoundException)
            {
                return false;
            }

            return true;
        }

        public static bool DoesMethodExists(this IAmazonAPIGateway gateway, string apiId, string httpMethod, string resourceId)
        {
            try
            {
                gateway.WaitAndRetry(x => x.GetMethod(new GetMethodRequest() { RestApiId = apiId, HttpMethod = httpMethod.ToUpper(), ResourceId = resourceId }));
            }
            catch (NotFoundException)
            {
                return false;
            }

            return true;
        }

        public static IEnumerable<Model> BuildModelList(this IAmazonAPIGateway gateway, string apiId)
        {
            var modelList = new List<Model>();

            var result = gateway.WaitAndRetry(x => x.GetModels(new GetModelsRequest()
            {
                RestApiId = apiId,
                Limit = 500
            }));

            modelList.AddRange(result.Items);
            var position = result.Position;

            while (position != null)
            {
                var models = gateway.WaitAndRetry(x => x.GetModels(new GetModelsRequest()
                {
                    RestApiId = apiId,
                    Position = position,
                    Limit = 500
                }));

                modelList.AddRange(models.Items);
                position = models.Position;
            }

            return modelList;
        }

        public static IEnumerable<Resource> BuildResourceList(this IAmazonAPIGateway gateway, string apiId)
        {
            var resourceList = new List<Resource>();

            var result = gateway.WaitAndRetry(x => x.GetResources(new GetResourcesRequest()
            {
                RestApiId = apiId,
                Limit = 500
            }));

            resourceList.AddRange(result.Items);
            var position = result.Position;

            while (position != null)
            {
                var resources = gateway.WaitAndRetry(x => x.GetResources(new GetResourcesRequest()
                {
                    RestApiId = apiId,
                    Position = position,
                    Limit = 500
                }));

                resourceList.AddRange(resources.Items);
                position = resources.Position;
            }

            return resourceList;
        }

        public static TResult WaitAndRetry<TResult>(this IAmazonAPIGateway gateway, Func<IAmazonAPIGateway, TResult> action)
        {
            var policy = Policy.Handle<TooManyRequestsException>()
                .WaitAndRetry(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3)
                });

            var result = policy.ExecuteAndCapture(() => action(gateway));

            if (result.FinalException != null)
                throw result.FinalException;

            return result.Result;
        }
    }
}
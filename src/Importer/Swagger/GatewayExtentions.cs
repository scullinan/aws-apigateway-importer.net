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
            return PageWaitAndRetry<Model>(gateway, (x, limit, pos) => x.GetModels(new GetModelsRequest()
            {
                RestApiId = apiId,
                Position = pos,
                Limit = limit
            }));
        }

        public static IEnumerable<Resource> BuildResourceList(this IAmazonAPIGateway gateway, string apiId)
        {
            return PageWaitAndRetry<Resource>(gateway, (x, limit, pos) => x.GetResources(new GetResourcesRequest()
            {
                RestApiId = apiId,
                Position = pos,
                Limit = limit
            }));
        }

        public static TResult WaitAndRetry<TResult>(this IAmazonAPIGateway gateway, Func<IAmazonAPIGateway, TResult> action)
        {
            var policy = Policy.Handle<TooManyRequestsException>()
                .WaitAndRetry(new[]
                {
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(4)
                });

            var result = policy.ExecuteAndCapture(() => action(gateway));

            if (result.FinalException != null)
                throw result.FinalException;

            return result.Result;
        }

        public static IEnumerable<TResult> PageWaitAndRetry<TResult>(this IAmazonAPIGateway gateway, Func<IAmazonAPIGateway, int, string, dynamic> action, int limit = 500)
        {
            var resourceList = new List<TResult>();

            var result = gateway.WaitAndRetry(x => action(x, limit, null));

            resourceList.AddRange(result.Items);
            var position = result.Position;

            while (position != null)
            {
                dynamic resources = gateway.WaitAndRetry(x => action(x, limit, position));

                resourceList.AddRange(resources.Items);
                position = resources.Position;
            }

            return resourceList;
        }
    }
}
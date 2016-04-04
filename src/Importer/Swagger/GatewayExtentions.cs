using System;
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
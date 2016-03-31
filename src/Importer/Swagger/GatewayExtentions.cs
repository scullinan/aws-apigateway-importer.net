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
                var response = gateway.GetModel(new GetModelRequest() {RestApiId = apiId, ModelName = modelName});
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public static bool DoesMethodExists(this IAmazonAPIGateway gateway, string apiId, string httpMethod, string resourceId)
        {
            try
            {
                var response = gateway.GetMethod(new GetMethodRequest() { RestApiId = apiId, HttpMethod = httpMethod.ToUpper(), ResourceId = resourceId });
            }
            catch (NotFoundException ex)
            {
                return false;
            }

            return true;
        }

        public static PolicyResult<TResult> WaitAndRetry<TResult>(this IAmazonAPIGateway gateway, Func<IAmazonAPIGateway, TResult> action)
        {
            var count = 0;
            var policy = Policy.Handle<TooManyRequestsException>()
                .WaitAndRetry(new[] {
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(4)
                }, (x, y) =>
                {
                    count++;
                });

            return policy.ExecuteAndCapture(() => action(gateway));
        }
    }
}
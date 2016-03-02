using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.APIGateway.Model;

namespace AWS.APIGateway.Impl
{
    public partial class ApiGatewaySdkSwaggerApiImporter
    {
        private void CreateDeployment(string apiId, DeploymentConfig config)
        {
            var request = new CreateDeploymentRequest
            {
                RestApiId = apiId,
                Description = config.Description,
                StageName = config.StageName,
                StageDescription = config.StageDescription,
                CacheClusterEnabled = (bool) config.Caching?.Enabled,
                CacheClusterSize = config.Caching?.ClusterSize
            };

            Log.InfoFormat("Creating deployment for API : {0}", apiId);
            Client.CreateDeployment(request);

            var builder = PatchOperationBuilder.With();

            if (config.Logging?.Enabled == true)
            {
                if (!string.IsNullOrEmpty(config.Logging?.CloudwatchRoleArn))
                {
                    var accountOps = PatchOperationBuilder.With()
                        .Operation(Operations.Replace, "/cloudwatchRoleArn", config.Logging.CloudwatchRoleArn)
                        .ToList();

                    Client.UpdateAccount(new UpdateAccountRequest() { PatchOperations = accountOps });

                    builder
                    .Operation(Operations.Replace, "/*/*/metrics/enabled", config.Logging?.MetricsEnabled.ToString())
                    .Operation(Operations.Replace, "/*/*/logging/loglevel", config.Logging?.LoggingLevel)
                    .Operation(Operations.Replace, "/*/*/logging/dataTrace", config.Logging?.DataTraceEnabled.ToString());
                }
                else
                {
                    Log.WarnFormat("CloudWatchRoleArn must be specified for logging");
                }
            }

            if (config.Caching?.Enabled == true)
            {
                builder
                    .Operation(Operations.Replace, "/*/*/caching/enabled", config.Caching?.Enabled.ToString())
                    .Operation(Operations.Replace, "/*/*/caching/ttlInSeconds", config.Caching?.CacheTtlInSeconds.ToString())
                    .Operation(Operations.Replace, "/*/*/caching/dataEncrypted", config.Caching?.CacheDataEncrypted.ToString());
            }

            builder
                .Operation(Operations.Replace, "/*/*/throttling/burstLimit", config.Throttling?.BurstLimit.ToString())
                .Operation(Operations.Replace, "/*/*/throttling/rateLimit", config.Throttling?.RateLimit.ToString());

            Client.UpdateStage(new UpdateStageRequest()
            {
                RestApiId = apiId,
                StageName = config.StageName,
                PatchOperations = builder.ToList()
            });
        }
    }
}


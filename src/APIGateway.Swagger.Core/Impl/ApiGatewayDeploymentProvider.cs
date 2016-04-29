using System.Collections.Generic;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using log4net;

namespace APIGateway.Management.Impl
{
    public class ApiGatewayDeploymentProvider : IApiGatewayDeploymentProvider
    {
        protected ILog Log = LogManager.GetLogger(typeof(ApiGatewayDeploymentProvider));
        private readonly IAmazonAPIGateway gateway;

        public ApiGatewayDeploymentProvider(IAmazonAPIGateway gateway)
        {
            this.gateway = gateway;
        }

        public void CreateDeployment(string apiId, DeploymentDocument config)
        {
            var request = new CreateDeploymentRequest
            {
                RestApiId = apiId,
                Description = config.Description,
                StageName = config.StageName,
                StageDescription = config.StageDescription,
                CacheClusterEnabled = (bool)config.Caching?.Enabled,
                CacheClusterSize = config.Caching?.ClusterSize
            };

            Log.InfoFormat("Creating deployment for API : {0}", apiId);
            gateway.CreateDeployment(request);

            var builder = PatchOperationBuilder.With();

            if (config.Logging?.Enabled == true)
            {
                if (!string.IsNullOrEmpty(config.Logging?.CloudwatchRoleArn))
                {
                    var accountOps = PatchOperationBuilder.With()
                        .Operation(Operations.Replace, Paths.CloudwatchRoleArn, config.Logging.CloudwatchRoleArn)
                        .ToList();

                    gateway.WaitAndRetry(x => x.UpdateAccount(new UpdateAccountRequest() { PatchOperations = accountOps }));

                    builder
                        .Operation(Operations.Replace, Paths.Logging.MetricsEnabled, config.Logging?.MetricsEnabled.ToString())
                        .Operation(Operations.Replace, Paths.Logging.LogLevel, config.Logging?.LoggingLevel)
                        .Operation(Operations.Replace, Paths.Logging.DataTrace, config.Logging?.DataTraceEnabled.ToString());
                }
                else
                    Log.WarnFormat("CloudWatchRoleArn must be specified for logging");
            }

            //Caching
            builder
                .Operation(Operations.Replace, Paths.Caching.Enabled, config.Caching?.Enabled.ToString())
                .Operation(Operations.Replace, Paths.Caching.TtlInSeconds, config.Caching?.TtlInSeconds.ToString())
                .Operation(Operations.Replace, Paths.Caching.DataEncrypted, config.Caching?.DataEncrypted.ToString())
            //Throttling
                .Operation(Operations.Replace, Paths.Throttling.BurstLimit, config.Throttling?.BurstLimit.ToString())
                .Operation(Operations.Replace, Paths.Throttling.RateLimit, config.Throttling?.RateLimit.ToString());

            gateway.WaitAndRetry(x => x.UpdateStage(new UpdateStageRequest() {
                RestApiId = apiId,
                StageName = config.StageName,
                PatchOperations = builder.ToList()
            }));
        }

        public void CreateDomain(DeploymentDocument config)
        {
            if (config.Domain != null)
            {
                gateway.WaitAndRetry(x => x.CreateDomainName(new CreateDomainNameRequest() {
                    DomainName = config.Domain.DomainName,
                    CertificateBody = config.Domain.CetificateBody,
                    CertificateName = config.Domain.CetificateName,
                    CertificateChain = config.Domain.CetificateChain,
                    CertificatePrivateKey = config.Domain.CetificatePrivateKey
                }));
            }
        }

        public string CreateApiKey(string apiId, string name, string stage)
        {
            var result = gateway.WaitAndRetry(x => x.CreateApiKey(new CreateApiKeyRequest() {
                Enabled = true,
                Name = name,
                StageKeys = new List<StageKey>()
                {
                    new StageKey() {RestApiId = apiId, StageName = stage}
                }
            }));

            return result.Id;
        }

        public void DeleteApiKey(string key)
        {
            gateway.WaitAndRetry(x => x.DeleteApiKey(new DeleteApiKeyRequest()
            {
                ApiKey = key
            }));
        }
    }
}
using System;
using System.Collections.Generic;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using log4net;

namespace ApiGatewayImporter.Sdk.Impl
{
    public class ApiGatewaySdkApiImporter
    {
        protected AmazonAPIGatewayClient Client;
        protected ILog Log = LogManager.GetLogger(typeof(ApiGatewaySdkApiImporter));
        protected HashSet<string> ProcessedModels = new HashSet<string>();

        public ApiGatewaySdkApiImporter()
        {
            Client = new AmazonAPIGatewayClient();
        }

        protected CreateRestApiResponse CreateApi(string name, string description)
        {
            Log.InfoFormat("Creating API with Name {0}", name);

            var request = new CreateRestApiRequest {
                Name = name,
                Description = description
            };

            return Client.CreateRestApi(request);
        }

        protected Resource GetRootResource(RestApi api)
        {
            foreach (Resource resource in BuildResourceList(api))
            {
                if ("/".Equals(resource.Path))
                {
                    return resource;
                }
            }

            return null;
        }

        protected List<Resource> BuildResourceList(RestApi api)
        {
            var resourceList = new List<Resource>();

            var resources = Client.GetResources(new GetResourcesRequest()
            {
                RestApiId = api.Id,
                Limit = 500
            });

            resourceList.AddRange(resources.Items);

            //ToDo:Travese next link
            //while (resources.._isLinkAvailable("next")) {
            //{
            //    resources = resources.getNext();
            //    resourceList.addAll(resources.getItem());
            //}

            return resourceList;
        }

        protected string GetApiName(SwaggerDocument swagger, string fileName)
        {
            var title = swagger.Info.Title;
            return !string.IsNullOrEmpty(title) ? title : fileName;
        }
    }
}
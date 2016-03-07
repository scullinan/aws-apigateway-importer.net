using System.Collections.Generic;
using Amazon.APIGateway.Model;

namespace ApiGatewayImporter.Sdk.Impl.old
{
    //Resources
    public partial class ApiGatewaySdkSwaggerApiImporter
    {
        //Todo:Refactor
        protected Resource CreateResource(RestApi api, string parentResourceId, string part)
        {
            Resource existingResource = GetResource(api, parentResourceId, part);

            // create resource if doesn't exist
            if (existingResource == null)
            {
                Log.InfoFormat("Creating resource '{0}' on {1}", part, parentResourceId);

               var resource = Client.CreateResource(new CreateResourceRequest()
                {
                   RestApiId = api.Id,
                   ParentId = parentResourceId,
                   PathPart = part
                });

                return new Resource() {
                    Id = resource.Id,
                    ParentId = resource.ParentId,
                    PathPart = resource.PathPart,
                    Path = resource.Path,
                    ResourceMethods = resource.ResourceMethods
                };
            }

            var result = Client.GetResource(new GetResourceRequest()
            {
                RestApiId = api.Id,
                ResourceId = parentResourceId
            });

            return new Resource()
            {
                ParentId = result.ParentId,
                PathPart = result.PathPart,
                Id = result.Id,
                Path = result.Path,
                ResourceMethods = result.ResourceMethods 
            };
        }

        private void CreateResources(RestApi api, Resource rootResource, string basePath, IList<string> apiProduces, IDictionary<string, PathItem> paths, bool createMethods)
        {
            //build path tree

            foreach (var path in paths)
            {
                // create the resource tree
                Resource parentResource = rootResource;

                var fullPath = BuildResourcePath(basePath, path.Key); // prepend the base path to all paths
                var parts = fullPath.Split('/');

                for (int i = 1; i < parts.Length; i++)
                {
                    // exclude root resource as this will be created when the api is created
                    parentResource = CreateResource(api, parentResource.Id, parts[i]);
                }

                if (createMethods)
                {
                    // create methods on the leaf resource for each path
                    CreateMethods(api, parentResource, path.Value, apiProduces);
                }
            }
        }
    }
}

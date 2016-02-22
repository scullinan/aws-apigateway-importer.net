using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.APIGateway.Model;

namespace aws_apigateway_importer.net.Impl
{
    //Resources
    public partial class ApiGatewaySdkSwaggerApiImporter
    {
        //Todo:Refactor
        protected async Task<Resource> CreateResource(RestApi api, string parentResourceId, string part)
        {
            Resource existingResource = await GetResource(api, parentResourceId, part);

            // create resource if doesn't exist
            if (existingResource == null)
            {
                Log.InfoFormat("Creating resource '{0}' on {1}", part, parentResourceId);

               var resource = await Client.CreateResourceAsync(new CreateResourceRequest()
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

            var result = await Client.GetResourceAsync(new GetResourceRequest()
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

        private async Task CreateResources(RestApi api, Resource rootResource, string basePath, IList<string> apiProduces, IDictionary<string, PathItem> paths, bool createMethods)
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
                    parentResource = await CreateResource(api, parentResource.Id, parts[i]);
                }

                if (createMethods)
                {
                    // create methods on the leaf resource for each path
                    await CreateMethods(api, parentResource, path.Value, apiProduces);
                }
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using APIGateway.Swagger;
using log4net;

namespace APIGateway.Management.Impl
{
    public class ApiGatewayResourceProvider : IApiGatewayResourceProvider
    {
        private readonly IAmazonAPIGateway gateway;
        private readonly IApiGatewayMethodProvider methodProvider;
        readonly ILog log = LogManager.GetLogger(typeof(ApiGatewayResourceProvider));

        public ApiGatewayResourceProvider(IAmazonAPIGateway gateway, IApiGatewayMethodProvider methodProvider)
        {
            this.gateway = gateway;
            this.methodProvider = methodProvider;
        }

        public void CreateResources(RestApi api, Resource rootResource, SwaggerDocument swagger, bool createMethods)
        {
            //build path tree
            foreach (var path in swagger.Paths)
            {
                // create the resource tree
                Resource parentResource = rootResource;

                var fullPath = BuildResourcePath(swagger.BasePath, path.Key); // prepend the base path to all paths
                var parts = fullPath.Split('/');

                for (int i = 1; i < parts.Length; i++)
                {
                    // exclude root resource as this will be created when the api is created
                    parentResource = CreateResource(api, parentResource.Id, parts[i]);
                }

                if (createMethods)
                {
                    // create methods on the leaf resource for each path
                    methodProvider.CreateMethods(api, swagger, parentResource, path.Value, swagger.Produces);
                }
            }
        }

        public void UpdateResources(RestApi api, Resource rootResource, SwaggerDocument swagger)
        {
            CreateResources(api, rootResource, swagger, false);
        }

        public void CleanupResources(RestApi api, SwaggerDocument swagger)
        {
            log.Info("Cleaning up removed resources");
            
            var paths = BuildResourceListFromSwagger(swagger.Paths.Keys, swagger.BasePath);
            CleanupResources(api, paths);
        }

        public void DeleteResources(RestApi api)
        {
            log.Info("Deleting all resources");

            var resources = gateway.BuildResourceList(api.Id);
            var deleteResources = resources.Where(x => !x.Path.Equals("/"));
            foreach (var resource in deleteResources)
            {
                log.Info("Deleting resource {0}" + resource.Path);

                try
                {
                    gateway.WaitAndRetry(x => x.DeleteResource(new DeleteResourceRequest()
                    {
                        RestApiId = api.Id,
                        ResourceId = resource.Id
                    }));
                }
                catch(NotFoundException) { }
            }
        }

        private Resource CreateResource(RestApi api, string parentResourceId, string part)
        {
            Resource existingResource = GetResource(api, parentResourceId, part);

            // create resource if doesn't exist
            if (existingResource == null)
            {
                log.InfoFormat("Creating resource '{0}' on {1}", part, parentResourceId);

                var resource = gateway.WaitAndRetry(x => x.CreateResource(new CreateResourceRequest()
                {
                    RestApiId = api.Id,
                    ParentId = parentResourceId,
                    PathPart = part
                }));

                return new Resource()
                {
                    Id = resource.Id,
                    ParentId = resource.ParentId,
                    PathPart = resource.PathPart,
                    Path = resource.Path,
                    ResourceMethods = resource.ResourceMethods
                };
            }

            return new Resource()
            {
                Id = existingResource.Id,
                ParentId = existingResource.ParentId,
                PathPart = existingResource.PathPart,
                Path = existingResource.Path,
                ResourceMethods = existingResource.ResourceMethods
            };
        }

        private void CleanupResources(RestApi api, List<string> paths)
        {
            log.Info("Cleaning up removed resources");

            var resources = gateway.BuildResourceList(api.Id);
            var deleteResources = resources.Where(x => !paths.Contains(x.PathPart) && !x.Path.Equals("/"));
            foreach (var resource in deleteResources)
            {
                log.Info("Removing deleted resource {0}" + resource.Path);
                gateway.WaitAndRetry(x => x.DeleteResource(new DeleteResourceRequest()
                {
                    RestApiId = api.Id,
                    ResourceId = resource.Id
                }));
            }
        }

       /*Build the full resource path, including base path, add any missing leading '/', remove any trailing '/',
       and remove any double '/'
       @param basePath the base path
       @param resourcePath the resource path
       @return the full path */
        private string BuildResourcePath(string basePath, string resourcePath)
        {
            if (basePath == null)
            {
                basePath = "";
            }

            string @base = TrimSlashes(basePath);

            if (!@base.Equals(""))
            {
                @base = "/" + @base;
            }

            var result = (@base + "/" + TrimSlashes(resourcePath)).TrimEnd('/');
            if (result.Equals(""))
            {
                result = "/";
            }

            return result;
        }

        private string TrimSlashes(string path)
        {
            return path.TrimEnd('/').TrimStart('/');
        }

        private Resource GetResource(RestApi api, string parentResourceId, string pathPart)
        {
            foreach (Resource resource in gateway.BuildResourceList(api.Id))
            {
                if (PathEquals(pathPart, resource.PathPart) && resource.ParentId.Equals(parentResourceId))
                {
                    return resource;
                }
            }

            return null;
        }

        private bool PathEquals(string p1, string p2)
        {
            return (string.IsNullOrEmpty(p1) && string.IsNullOrEmpty(p2)) || p1.Equals(p2);
        }

        private List<string> BuildResourceListFromSwagger(IEnumerable<string> paths, string basePath)
        {
            if (string.IsNullOrEmpty(basePath))
            {
                basePath = "/";
            }

            var resourceSet = new List<string>();

            foreach (var path in paths)
            {
                resourceSet.AddRange(path.Split('/'));
            }

            resourceSet.AddRange(basePath.Split('/'));
            return resourceSet;
        }
    }
}
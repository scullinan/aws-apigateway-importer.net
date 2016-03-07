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

        protected void DeleteDefaultModels(RestApi api)
        {
            var list = BuildModelList(api);
            list.ForEach(model => {
                Log.InfoFormat("Removing default model {0}", model.Name);

                try
                {
                    Client.DeleteModel(new DeleteModelRequest() {
                        RestApiId = api.Id,
                        ModelName = model.Name
                    });
                }
                catch (Exception)
                {
                    // ignored
                } // todo: temporary catch until API fix
            });
        }

        protected List<Model> BuildModelList(RestApi api)
        {
            var modelList = new List<Model>();
            
            var resources = Client.GetModels(new GetModelsRequest()
            {
                RestApiId = api.Id,
                Limit = 500
                
            });

            var response = Client.GetModels(new GetModelsRequest()
            {
                RestApiId = api.Id
            });

            modelList.AddRange(response.Items);

            //ToDo:Travese next link
            //while (models._isLinkAvailable("next"))
            //{
            //    models = models.getNext();
            //    modelList.addAll(models.getItem());
            //}

            return modelList;
        }

        protected void CreateModel(RestApi api, string modelName, string description, string schema, string modelContentType)
        {
            if (schema == null) throw new ArgumentNullException(nameof(schema));
            ProcessedModels.Add(modelName);

            var input = new CreateModelRequest
            {
                RestApiId = api.Id,
                Name = modelName,
                Description = description,
                ContentType = modelContentType,
                Schema = schema
            };

            Client.CreateModel(input);
        }

        /* Build the full resource path, including base path, add any missing leading '/', remove any trailing '/',
        and remove any double '/'
        @param basePath the base path
        @param resourcePath the resource path
        @return the full path */
        protected string BuildResourcePath(string basePath, string resourcePath)
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

        protected Resource GetResource(RestApi api, string parentResourceId, string pathPart)
        {
            foreach (Resource resource in BuildResourceList(api))
            {
                if (PathEquals(pathPart, resource.PathPart) && resource.ParentId.Equals(parentResourceId))
                {
                    return resource;
                }
            }

            return null;
        }

        protected bool PathEquals(string p1, string p2)
        {
            return (string.IsNullOrEmpty(p1) && string.IsNullOrEmpty(p2)) || p1.Equals(p2);
        }

        protected string GetStringValue(object obj)
        {
            return obj == null ? null : Convert.ToString(obj);  // use null value instead of "null"
        }
    }
}
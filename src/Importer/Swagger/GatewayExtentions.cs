using Amazon.APIGateway;
using Amazon.APIGateway.Model;

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
            catch (NotFoundException ex)
            {
                return false;
            }

            return true;
        }
    }
}
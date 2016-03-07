namespace ApiGatewayImporter.Sdk.Impl.old
{
    public partial class ApiGatewaySdkSwaggerApiImporter
    {
        private string GetApiName(SwaggerDocument swagger, string fileName)
        {
            var title = swagger.Info.Title;
            return !string.IsNullOrEmpty(title) ? title : fileName;
        }
    }
}

using System;
using System.Text.RegularExpressions;

namespace Importer.Swagger.Aws.Impl
{
    public class ModelNameResolver : IModelNameResolver
    {
        private const string ModelNameFormat = "{0}x{1}x{2}";

        public string GetModelName(string resourceName, string method, Response response)
        {
            if (string.IsNullOrEmpty(response.Description))
            {
                return $"Model{Guid.NewGuid().ToString().Substring(0, 8)}";
            }

            return string.Format(ModelNameFormat, resourceName.ToUpper(), method.ToUpper(), Sanitize(response.Description));
        }

        public string GetModelName(string refPath)
        {
            return string.IsNullOrEmpty(@refPath) ? string.Empty : @refPath.Substring(@refPath.LastIndexOf('/') + 1);
        }

        public string Sanitize(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "x", RegexOptions.Compiled);
        }
    }
}

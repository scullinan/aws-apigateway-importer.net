using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace APIGateway.Swagger
{
    public class SwaggerContractResolver : CamelCasePropertyNamesContractResolver
    {
        private static readonly JsonConverter converter = new DefinitionsConverter();
        private static Type type = typeof(IDictionary<string, Schema>);

        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (objectType == null || !type.IsAssignableFrom(objectType))
            {
                return base.ResolveContractConverter(objectType);
            }

            return converter;
        }
    }
}
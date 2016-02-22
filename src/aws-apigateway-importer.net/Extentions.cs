using System;
using System.Collections.Generic;
using Amazon.APIGateway.Model;

namespace aws_apigateway_importer.net
{
    public static class Extentions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }

        public static RestApi RestApi(this CreateRestApiResponse response)
        {
            return new RestApi() {
                Id = response.Id,
                Name = response.Name,
                CreatedDate = response.CreatedDate,
                Description = response.Description
            };
        }
    }
}
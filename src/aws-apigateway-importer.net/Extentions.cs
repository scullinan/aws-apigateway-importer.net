using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.APIGateway.Model;
using Newtonsoft.Json.Linq;

namespace AWS.APIGateway
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
            return new RestApi()
            {
                Id = response.Id,
                Name = response.Name,
                CreatedDate = response.CreatedDate,
                Description = response.Description
            };
        }

        public static IDictionary<string, object> ToDictionary(this JObject @object)
        {
            var result = @object.ToObject<Dictionary<string, object>>();

            var jObjectKeys = (from r in result
                let key = r.Key
                let value = r.Value
                where value.GetType() == typeof (JObject)
                select key).ToList();

            var jArrayKeys = (from r in result
                let key = r.Key
                let value = r.Value
                where value.GetType() == typeof (JArray)
                select key).ToList();

            jArrayKeys.ForEach(
                key => result[key] = ((JArray) result[key]).Values().Select(x => ((JValue) x).Value).ToArray());
            jObjectKeys.ForEach(key => result[key] = ToDictionary(result[key] as JObject));

            return result;
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IDictionary<string, object> dictionary,
            string key)
        {
            if (dictionary.ContainsKey(key))
                return dictionary[key] as Dictionary<TKey, TValue>;

            return null;
        }
    }
}
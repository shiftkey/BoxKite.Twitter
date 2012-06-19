using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BoxKite.Twitter.Extensions
{
    public static class WebResponseExtensions
    {
        public static IEnumerable<TOutput> MapTo<TResponse, TOutput>(this HttpResponseMessage response, Func<TResponse, IEnumerable<TOutput>> callback)
        {
            var content = response.Content.ReadAsStringAsync().Result;

            try
            {
                var json = JObject.Parse(content);
                if (json["error"] != null)
                {
                    return Enumerable.Empty<TOutput>();
                }
            }
            catch
            {
                // its an array of items
            }
            
            var objects = JsonConvert.DeserializeObject<TResponse>(content);
            return callback(objects);
        }

        public static TOutput MapTo<TResponse, TOutput>(this HttpResponseMessage response, Func<TResponse, TOutput> callback)
        {
            var content = response.Content.ReadAsStringAsync().Result;

            try
            {
                var json = JObject.Parse(content);
                if (json["error"] != null)
                {
                    return default(TOutput);
                }
            }
            catch
            {
                // its an array of items
            }
            
            var objects = JsonConvert.DeserializeObject<TResponse>(content);
            return callback(objects);
        }

        public static string GetContent(this HttpResponseMessage response)
        {
            return response.Content.ReadAsStringAsync().Result; // TODO: this feels wrong
        }
    }
}
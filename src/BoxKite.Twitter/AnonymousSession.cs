using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BoxKite.Twitter
{
    public class AnonymousSession : IAnonymousSession
    {
        public Task<HttpResponseMessage> GetAsync(string relativeUrl, SortedDictionary<string, string> parameters)
        {
            var querystring = parameters.Aggregate("", (current, entry) => current + (entry.Key + "=" + entry.Value + "&"));

            var fullUrl = "https://api.twitter.com/1/" + relativeUrl;

            if (!string.IsNullOrWhiteSpace(querystring))
            {
                fullUrl += "?" + querystring;
            }

            var client = new HttpClient();
            return client.GetAsync(new Uri(fullUrl));
        }

        public Task<HttpResponseMessage> GetAsyncFull(string fullUrl)
        {
            var client = new HttpClient();
            return client.GetAsync(new Uri(fullUrl));
        }
    }
}
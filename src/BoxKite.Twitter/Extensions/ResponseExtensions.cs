using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BoxKite.Twitter.Mappings;
using BoxKite.Twitter.Models.Internal;
using Newtonsoft.Json;
using Tweet = BoxKite.Twitter.Models.Tweet;
using User = BoxKite.Twitter.Models.User;

namespace BoxKite.Twitter.Extensions
{
    internal static class ResponseExtensions
    {
        internal static IEnumerable<long> MapToIds(this Task<HttpResponseMessage> task)
        {
            if (task.IsFaulted || task.IsCanceled)
                return Enumerable.Empty<long>();

            var result = task.Result;
            if (!result.IsSuccessStatusCode)
                return Enumerable.Empty<long>();

            var content = result.Content.ReadAsStringAsync();

            var text = content.Result;
            var ids = JsonConvert.DeserializeObject<Ids>(text);
            return ids.ids;
        }

        internal static User MapToSingleUser(this Task<HttpResponseMessage> c)
        {
            if (c.IsFaulted || c.IsCanceled)
                return null;

            var result = c.Result;
            if (!result.IsSuccessStatusCode)
                return null;

            var text = result.Content.ReadAsStringAsync().Result;
            return text.GetSingleUser();
        }

        internal static Tweet MapToSingleTweet(this Task<HttpResponseMessage> c)
        {
            if (c.IsFaulted || c.IsCanceled)
                return null;

            var result = c.Result;
            if (!result.IsSuccessStatusCode)
                return null;

            var text = result.Content.ReadAsStringAsync().Result;
            return text.GetSingle();
        }

        internal static IObservable<Tweet> MapToListOfTweets(this Task<HttpResponseMessage> c)
        {
            if (c.IsFaulted || c.IsCanceled)
                return Observable.Empty<Tweet>();

            var result = c.Result;
            if (!result.IsSuccessStatusCode)
                return Observable.Empty<Tweet>();

            var text = result.Content.ReadAsStringAsync().Result;
            return text.GetList();
        }

        internal static bool MapToBoolean(this Task<HttpResponseMessage> c)
        {
            if (c.IsFaulted || c.IsCanceled)
                return false;

            var result = c.Result;
            return result.IsSuccessStatusCode;
        }
    }
}
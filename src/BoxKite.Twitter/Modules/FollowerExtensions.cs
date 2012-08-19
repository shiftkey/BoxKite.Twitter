using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BoxKite.Twitter.Extensions;

// ReSharper disable CheckNamespace
namespace BoxKite.Twitter
// ReSharper enable CheckNamespace
{
    public static class FollowerExtensions
    {
        public static IObservable<long> GetFollowers(this IUserSession session)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                      { "cursor", "-1" }
                                 };

            return session.GetAsync(Api.Resolve("/1/followers/ids.json"), parameters)
                             .ContinueWith(t => t.MapToIds())
                             .ToObservable()
                             .SelectMany(c => c);
        }

        public static IObservable<Models.User> GetFollowers(this IUserSession session, IEnumerable<long> ids)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                      { "include_entities", "true" },
                                      { "user_id", string.Join(",", ids.ToArray()) }
                                 };

            return session.PostAsync(Api.Resolve("/1/users/lookup.json"), parameters)
                          .ContinueWith(c => GetUsers(c))
                          .ToObservable()
                          .SelectMany(c => c);
        }

        private static IEnumerable<Models.User> GetUsers(Task<HttpResponseMessage> task)
        {
            // TODO: how to handle errors properly
            var result = task.Result;

            if (result.IsSuccessStatusCode)
            {
                //var body = await result.Content.ReadAsStringAsync();
                //var ids = JsonConvert.DeserializeObject<Ids>(body);
                //return ids.ids;
            }

            return Enumerable.Empty<Models.User>();
        }


    }
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BoxKite.Twitter.Extensions;
using BoxKite.Twitter.Mappings;
using BoxKite.Twitter.Models;

// ReSharper disable CheckNamespace
namespace BoxKite.Twitter.Modules
// ReSharper restore CheckNamespace
{
    public static class UsersExtensions
    {
        public static IObservable<User> GetProfile(this IUserSession session, string screenName)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                     {"screen_name", screenName},
                                     {"include_entities", "true"},
                                 };
            var url = Api.Resolve("/1/users/show.json");
            return session.GetAsync(url, parameters)
                          .ContinueWith(c => ProcessSingleResponse(c))
                          .ToObservable();
        }

        private static User ProcessSingleResponse(Task<HttpResponseMessage> c)
        {
            if (c.IsFaulted || c.IsCanceled)
                return null;

            var result = c.Result;
            if (!result.IsSuccessStatusCode)
                return null;

            var text = result.Content.ReadAsStringAsync().Result;
            return text.GetSingleUser();
        }

        public static IObservable<User> GetProfile(this IUserSession session, long id)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                     {"user_id", id.ToString()},
                                     {"include_entities", "true"},
                                 };
            var url = Api.Resolve("/1/users/show.json");
            return session.GetAsync(url, parameters)
                          .ContinueWith(c => ProcessSingleResponse(c))
                          .ToObservable();
        }

        public static IObservable<User> GetProfile(this IAnonymousSession session, string screenName)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                     {"screen_name", screenName},
                                     {"include_entities", "true"},
                                 };
            var url = Api.Resolve("/1/users/show.json");
            return session.GetAsync(url, parameters)
                          .ContinueWith(c => ProcessSingleResponse(c))
                          .ToObservable();
        }
    }
}

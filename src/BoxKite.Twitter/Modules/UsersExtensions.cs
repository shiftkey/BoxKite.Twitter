using System;
using System.Collections.Generic;
using System.Reactive.Threading.Tasks;
using BoxKite.Twitter.Extensions;
using BoxKite.Twitter.Models;

// ReSharper disable CheckNamespace
namespace BoxKite.Twitter
// ReSharper enable CheckNamespace
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
                          .ContinueWith(c => c.MapToSingleUser())
                          .ToObservable();
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
                          .ContinueWith(c => c.MapToSingleUser())
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
                          .ContinueWith(c => c.MapToSingleUser())
                          .ToObservable();
        }

        public static IObservable<User> GetVerifyCredentials(this IUserSession session)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                     {"include_entities", "true"},
                                 };
            var url = Api.Resolve("/1/account/verify_credentials.json");
            return session.GetAsync(url, parameters)
                          .ContinueWith(c => c.MapToSingleUser())
                          .ToObservable();
        }
    }
}

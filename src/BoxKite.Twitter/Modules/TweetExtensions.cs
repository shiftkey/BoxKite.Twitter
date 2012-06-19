using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BoxKite.Twitter.Extensions;
using BoxKite.Twitter.Mappings;

// ReSharper disable CheckNamespace
namespace BoxKite.Twitter
// ReSharper restore CheckNamespace
{
    // TODO: upload image overload

    public static class TweetExtensions
    {
        public static IObservable<Models.Tweet> Tweet(this IUserSession session, string text)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                     { "status", text},
                                     { "trim_user","true" },
                                     { "include_entities", "true" }
                                 };
            return session.PostAsync(Api.Resolve("/1/statuses/update.json"), parameters)
                          .ContinueWith(c => ProcessSingleResponse(c))
                          .ToObservable();
        }

        private static Models.Tweet ProcessSingleResponse(Task<HttpResponseMessage> c)
        {
            if (c.IsFaulted || c.IsCanceled)
                return null;

            var result = c.Result;
            if (!result.IsSuccessStatusCode)
                return null;

            var text = result.Content.ReadAsStringAsync().Result;
            return text.GetSingle();
        }

        public static IObservable<Models.Tweet> Tweet(this IUserSession session, string text, double latitude, double longitude)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                     {"status", text },
                                     {"lat", latitude.ToString()},
                                     {"long", longitude.ToString()},
                                 };
             return session.PostAsync(Api.Resolve("/1/statuses/update.json"), parameters)
                           .ContinueWith(c => ProcessSingleResponse(c))
                           .ToObservable();
        }

        public static IObservable<bool> Delete(this IUserSession session, string id)
        {
            var url = Api.Resolve("/1/statuses/destroy/{0}.json", id);
            var parameters = new SortedDictionary<string, string>();
            return session.PostAsync(url, parameters)
                          .ContinueWith(c => ProcessBool(c))
                          .ToObservable();
        }

        public static IObservable<Models.Tweet> Reply(this IUserSession session, Models.Tweet tweet, string text)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                     {"status", text },
                                     {"in_reply_to_status_id", tweet.Id}
                                 };
            return session.PostAsync(Api.Resolve("/1/statuses/update.json"), parameters)
                          .ContinueWith(c => ProcessSingleResponse(c))
                          .ToObservable();
        }

        public static IObservable<Models.Tweet> Reply(this IUserSession session, Models.Tweet tweet, string text, double latitude, double longitude)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                     {"status", text },
                                     {"lat", latitude.ToString()},
                                     {"long", longitude.ToString()},
                                     {"in_reply_to_status_id", tweet.Id}
                                 };
            return session.PostAsync(Api.Resolve("/1/statuses/update.json"), parameters)
                          .ContinueWith(c => ProcessSingleResponse(c))
                          .ToObservable();
        }

        public static IObservable<Models.Tweet> Retweet(this IUserSession session, Models.Tweet tweet)
        {
            var parameters = new SortedDictionary<string, string>();
            var path = Api.Resolve("/1/statuses/retweet/{0}.json", tweet.Id);

            return session.PostAsync(path, parameters)
                          .ContinueWith(c => ProcessSingleResponse(c))
                          .ToObservable();
        }

        public static Models.Tweet UndoRetweet(this IUserSession session, Models.Tweet tweet)
        {
            return null;
        }

        private static bool ProcessBool(Task<HttpResponseMessage> c)
        {
            if (c.IsFaulted || c.IsCanceled)
                return false;

            var result = c.Result;
            return result.IsSuccessStatusCode;
        }
    }
}

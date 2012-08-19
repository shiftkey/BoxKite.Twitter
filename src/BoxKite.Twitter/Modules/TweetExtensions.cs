using System;
using System.Collections.Generic;
using System.Reactive.Threading.Tasks;
using BoxKite.Twitter.Extensions;
using BoxKite.Twitter.Models;

// ReSharper disable CheckNamespace
namespace BoxKite.Twitter
// ReSharper restore CheckNamespace
{
    // TODO: upload image overload

    public static class TweetExtensions
    {
        public static IObservable<Tweet> Tweet(this IUserSession session, string text)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                     { "status", text},
                                     { "trim_user","true" },
                                     { "include_entities", "true" }
                                 };
            return session.PostAsync(Api.Resolve("/1/statuses/update.json"), parameters)
                          .ContinueWith(c => c.MapToSingleTweet())
                          .ToObservable();
        }

        public static IObservable<Tweet> Tweet(this IUserSession session, string text, double latitude, double longitude)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                     {"status", text },
                                     {"lat", latitude.ToString()},
                                     {"long", longitude.ToString()},
                                 };
             return session.PostAsync(Api.Resolve("/1/statuses/update.json"), parameters)
                           .ContinueWith(c => c.MapToSingleTweet())
                           .ToObservable();
        }

        public static IObservable<bool> Delete(this IUserSession session, string id)
        {
            var url = Api.Resolve("/1/statuses/destroy/{0}.json", id);
            var parameters = new SortedDictionary<string, string>();
            return session.PostAsync(url, parameters)
                          .ContinueWith(c => c.MapToBoolean())
                          .ToObservable();
        }

        public static IObservable<Tweet> Reply(this IUserSession session, Tweet tweet, string text)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                     {"status", text },
                                     {"in_reply_to_status_id", tweet.Id}
                                 };
            return session.PostAsync(Api.Resolve("/1/statuses/update.json"), parameters)
                          .ContinueWith(c => c.MapToSingleTweet())
                          .ToObservable();
        }

        public static IObservable<Tweet> Reply(this IUserSession session, Tweet tweet, string text, double latitude, double longitude)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                     {"status", text },
                                     {"lat", latitude.ToString()},
                                     {"long", longitude.ToString()},
                                     {"in_reply_to_status_id", tweet.Id}
                                 };
            return session.PostAsync(Api.Resolve("/1/statuses/update.json"), parameters)
                          .ContinueWith(c => c.MapToSingleTweet())
                          .ToObservable();
        }

        public static IObservable<Tweet> Retweet(this IUserSession session, Tweet tweet)
        {
            var parameters = new SortedDictionary<string, string>();
            var path = Api.Resolve("/1/statuses/retweet/{0}.json", tweet.Id);

            return session.PostAsync(path, parameters)
                          .ContinueWith(c => c.MapToSingleTweet())
                          .ToObservable();
        }

        public static Tweet UndoRetweet(this IUserSession session, Tweet tweet)
        {
            return null;
        }

    }
}

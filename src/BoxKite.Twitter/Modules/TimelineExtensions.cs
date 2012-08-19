using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using BoxKite.Twitter.Extensions;
using BoxKite.Twitter.Models;

// ReSharper disable CheckNamespace
namespace BoxKite.Twitter
// ReSharper restore CheckNamespace
{
    public static class TimelineExtensions
    {
        public static IObservable<Tweet> GetMentions(this IUserSession session, string since = "")
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                     {"include_entities", "true"},
                                     {"include_rts", "true"},
                                     {"count", "200" }
                                 };

            if (!string.IsNullOrWhiteSpace(since))
            {
                parameters.Add("since_id", since);
            }

            var url = Api.Resolve("/1/statuses/mentions.json");
            return session.GetAsync(url, parameters)
                          .ContinueWith(c => c.MapToListOfTweets())
                          .ToObservable()
                          .SelectMany(c => c);
        }

        public static IObservable<Tweet> GetHomeTimeline(this IUserSession session, string since = "")
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                     {"count", "200"},
                                     {"include_entities", "true"},
                                     {"include_rts", "true"}
                                 };

            if (!string.IsNullOrWhiteSpace(since))
            {
                parameters.Add("since_id", since);
            }

            var url = Api.Resolve("/1/statuses/home_timeline.json");
            return session.GetAsync(url, parameters)
                          .ContinueWith(c => c.MapToListOfTweets())
                          .ToObservable()
                          .SelectMany(c => c);
        }

        public static IObservable<Tweet> GetRetweets(this IUserSession session, string since = "")
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                     {"count", "100"},
                                     {"include_entities", "true"},
                                 };

            if (!string.IsNullOrWhiteSpace(since))
            {
                parameters.Add("since_id", since);
            }

            var url = Api.Resolve("/1/statuses/retweeted_to_me.json");
            return session.GetAsync(url, parameters)
                          .ContinueWith(c => c.MapToListOfTweets())
                          .ToObservable()
                          .SelectMany(c => c);
        }
    }
}

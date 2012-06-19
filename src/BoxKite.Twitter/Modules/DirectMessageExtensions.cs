using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BoxKite.Twitter.Extensions;
using BoxKite.Twitter.Mappings;
using BoxKite.Twitter.Models;

// ReSharper disable CheckNamespace
namespace BoxKite.Twitter
// ReSharper restore CheckNamespace
{
    public static class DirectMessageExtensions
    {
        public static IObservable<DirectMessage> GetDirectMessages(this IUserSession session, string since = "")
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                     {"include_entities", "true"},
                                 };

            if (!string.IsNullOrWhiteSpace(since))
            {
                parameters.Add("since_id", since);
            }

            return session.GetAsync(Api.Resolve("/1/direct_messages.json"), parameters)
                          .ContinueWith(c => ProcessListResponse(c))
                          .ToObservable()
                          .SelectMany(c => c);
        }

        public static IObservable<DirectMessage> GetSentDirectMessages(this IUserSession session, string since = "")
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                     {"include_entities", "true"},
                                 };

            if (!string.IsNullOrWhiteSpace(since))
            {
                parameters.Add("since_id", since);
            }

            return session.GetAsync(Api.Resolve("/1/direct_messages/sent.json"), parameters)
                          .ContinueWith(c => ProcessListResponse(c))
                          .ToObservable()
                          .SelectMany(c => c);
        }

        public static IObservable<DirectMessage> SendMessage(this IUserSession session, string text, string screenName)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                     {"include_entities", "true"},
                                     {"screen_name", screenName},
                                     {"text", text}
                                 };

            return session.PostAsync(Api.Resolve("/1/direct_messages/new.json"), parameters)
                          .ContinueWith(c => ProcessSingleResponse(c))
                          .ToObservable();
        }

        public static IObservable<bool> DeleteMessage(this IUserSession session, string id)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                     {"include_entities", "true"},
                                     {"id", id}
                                 };

            return session.PostAsync(Api.Resolve("/1/direct_messages/destroy.json"), parameters)
                          .ContinueWith(c => ProcessBool(c))
                          .ToObservable();
        }

        private static IEnumerable<DirectMessage> ProcessListResponse(Task<HttpResponseMessage> c)
        {
            if (c.IsFaulted || c.IsCanceled)
                return Enumerable.Empty<DirectMessage>();

            var result = c.Result;
            if (!result.IsSuccessStatusCode)
                return Enumerable.Empty<DirectMessage>();

            var text = result.Content.ReadAsStringAsync().Result;
            return text.GetListDirectMessages();
        }

        private static DirectMessage ProcessSingleResponse(Task<HttpResponseMessage> c)
        {
            if (c.IsFaulted || c.IsCanceled)
                return null;

            var result = c.Result;
            if (!result.IsSuccessStatusCode)
                return null;

            var text = result.Content.ReadAsStringAsync().Result;
            return text.GetSingleDirectMessage();
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
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
// ReSharper enable CheckNamespace
{
    public static class SearchExtensions
    {
        public static IObservable<Tweet> SearchFor(this IAnonymousSession session, string text, string since = "")
        {
            var listOfRequests = new List<Task<HttpResponseMessage>>();

            for (var i = 1; i <= 1; i++)
            {
                var url = string.Format("http://search.twitter.com/search.json?q={0}&rpp=50&page={1}", text, i);
                if (!string.IsNullOrWhiteSpace(since))
                {
                    url += "&since_id=" + since;
                }
                var task = session.GetAsyncFull(url);
                listOfRequests.Add(task);
            }

            return Task.WhenAll(listOfRequests)
                       .ContinueWith(c => ParseResult(c))
                       .ToObservable()
                       .SelectMany(c => c);
        }

        private static IEnumerable<Tweet> ParseResult(Task<HttpResponseMessage[]> task)
        {
            Task<string>[] values = task.Result.Select(async c => await c.Content.ReadAsStringAsync()).ToArray();

            return Task.WhenAll(values).ContinueWith(c => c.Result.SelectMany(s => s.FromSearchResponse())).Result;
        }

        public static IObservable<SavedSearch> GetSavedSearches(this IUserSession session)
        {
            var parameters = new SortedDictionary<string, string>();
            return session.GetAsync(Api.Resolve("/1/report_spam.json"), parameters)
                          .ContinueWith(c => ProcessSet(c))
                          .ToObservable()
                          .SelectMany(c => c);
        }

        public static IObservable<SavedSearch> SaveSearch(this IUserSession session, string text)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                     {"query", text }
                                 };
            // TODO: this is wrong
            return session.PostAsync(Api.Resolve("/1/report_spam.json"), parameters)
                          .ContinueWith(c => ProcessResult(c))
                          .ToObservable();
        }

        public static IObservable<bool> DeleteSavedSearch(this IUserSession session, long id)
        {
            var savedSearch = Api.Resolve("/1/saved_searches/destroy/{0}.json", id);
            var parameters = new SortedDictionary<string, string>();
            return session.PostAsync(savedSearch, parameters)
                          .ContinueWith(c => ProcessBool(c))
                          .ToObservable();
        }

        private static IEnumerable<SavedSearch> ProcessSet(Task<HttpResponseMessage> c)
        {
            if (c.IsFaulted || c.IsCanceled)
                return Enumerable.Empty<SavedSearch>();

            var result = c.Result;
            if (!result.IsSuccessStatusCode)
                return Enumerable.Empty<SavedSearch>();

            var text = result.Content.ReadAsStringAsync().Result;
            return text.GetSavedSearchList();
        }

        private static SavedSearch ProcessResult(Task<HttpResponseMessage> c)
        {
            if (c.IsFaulted || c.IsCanceled)
                return null;

            var result = c.Result;
            if (!result.IsSuccessStatusCode)
                return null;

            var text = result.Content.ReadAsStringAsync().Result;
            return text.GetSavedSearch();
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

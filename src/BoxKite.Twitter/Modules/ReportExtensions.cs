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
    public static class ReportExtensions
    {
        public static IObservable<User> ReportSpam(this IUserSession session, string screenName)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                     {"screen_name", screenName}
                                 };
            return session.PostAsync(Api.Resolve("/1/report_spam.json"), parameters)
                          .ContinueWith(c => ProcessSingleResponse(c))
                          .ToObservable();
        }

        public static IObservable<User> BlockUser(this IUserSession session, string screenName)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                     {"screen_name", screenName}
                                 };
            return session.PostAsync(Api.Resolve("/1/blocks/create.json"), parameters)
                          .ContinueWith(c => ProcessSingleResponse(c))
                          .ToObservable();
        }

        public static IObservable<User> UnblockUser(this IUserSession session, string screenName)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                     {"screen_name", screenName}
                                 };
            return session.PostAsync(Api.Resolve("/1/blocks/destroy.json"), parameters)
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
    }
}

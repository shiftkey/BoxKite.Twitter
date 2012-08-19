using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BoxKite.Twitter.Extensions;
using BoxKite.Twitter.Mappings;
using Newtonsoft.Json;
using Tweet = BoxKite.Twitter.Models.Tweet;

namespace BoxKite.Twitter.Modules
{
    public static class FavouritesExtensions
    {
        public static IObservable<IEnumerable<Tweet>> GetFavourites(this IUserSession session)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                      { "cursor", "-1" }
                                 };

            return session.GetAsync(Api.Resolve("/1/favorites.json"), parameters)
                          .ContinueWith(c => ProcessListResponse(c))
                          .ToObservable();
        }

        private static IEnumerable<Tweet> ProcessListResponse(Task<HttpResponseMessage> c)
        {
            if (c.IsFaulted || c.IsCanceled)
                return Enumerable.Empty<Tweet>();

            var result = c.Result;
            if (!result.IsSuccessStatusCode)
                return Enumerable.Empty<Tweet>();

            var text = result.Content.ReadAsStringAsync().Result;
            return text.GetList().ToListObservable();
        }
    }
}

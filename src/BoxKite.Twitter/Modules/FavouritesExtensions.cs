using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using BoxKite.Twitter.Extensions;
using BoxKite.Twitter.Models;

namespace BoxKite.Twitter.Modules
{
    public static class FavouritesExtensions
    {
        public static IObservable<Tweet> GetFavourites(this IUserSession session)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                      { "cursor", "-1" }
                                 };

            return session.GetAsync(Api.Resolve("/1/favorites.json"), parameters)
                           .ContinueWith(c => c.MapToListOfTweets())
                           .ToObservable()
                           .SelectMany(c => c); // don't like this 
        }

        public static IObservable<Tweet> CreateFavourite(this IUserSession session, Tweet tweet)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                      { "cursor", "-1" }
                                 };

            var url = Api.Resolve("/1/favorites/create/{0}.json", tweet.Id);
            return session.PostAsync(url, parameters)
                          .ContinueWith(c => c.MapToSingleTweet())
                          .ToObservable();
        }

        public static IObservable<Tweet> DestroyFavourite(this IUserSession session, Tweet tweet)
        {
            var parameters = new SortedDictionary<string, string>
                                 {
                                      { "cursor", "-1" }
                                 };

            var url = Api.Resolve("/1/favorites/destroy/{0}.json", tweet.Id);
            return session.PostAsync(url, parameters)
                          .ContinueWith(c => c.MapToSingleTweet())
                          .ToObservable();
        }
    }
}

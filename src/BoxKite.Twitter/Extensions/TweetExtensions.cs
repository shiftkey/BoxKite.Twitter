using BoxKite.Twitter.Models;

namespace BoxKite.Twitter.Extensions
{
    public static class TweetExtensions
    {
        public static bool HasEntities(this Tweet tweet)
        {
            if (tweet.Hashtags.HasAny())
                return true;

            if (tweet.Media.HasAny())
                return true;

            if (tweet.Urls.HasAny())
                return true;

            return false;
        }
    }
}

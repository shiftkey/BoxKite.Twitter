using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using BoxKite.Twitter.Extensions;
using BoxKite.Twitter.Models;
using BoxKite.Twitter.Models.Internal;
using Newtonsoft.Json;
using Hashtag = BoxKite.Twitter.Models.Hashtag;
using Tweet = BoxKite.Twitter.Models.Tweet;
using Url = BoxKite.Twitter.Models.Url;
using User = BoxKite.Twitter.Models.User;

namespace BoxKite.Twitter.Mappings
{
    public static class TwitterMappings
    {
        public static IObservable<Tweet> GetList(this string body)
        {
            List<Models.Internal.Tweet> objects;
            try
            {
                objects = JsonConvert.DeserializeObject<List<Models.Internal.Tweet>>(body);
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();

                return Observable.Empty<Tweet>();
            }

            return objects.Select(MapFromInternalTweet).ToObservable();
        }

        internal static IEnumerable<User> GetListOfUsers(this string body)
        {
            List<Models.Internal.User> objects;
            try
            {
                objects = JsonConvert.DeserializeObject<List<Models.Internal.User>>(body);
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();

                return Enumerable.Empty<User>();
            }

            return objects.Select(MapUser);
        }

        internal static IEnumerable<DirectMessage> GetListDirectMessages(this string body)
        {
            try
            {
                var objects = JsonConvert.DeserializeObject<List<DM>>(body);
                return objects.Select(MapFromInternal);
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();

                return Enumerable.Empty<DirectMessage>();
            }
        }

        public static Tweet GetSingle(this string body)
        {
            try
            {
                var o = JsonConvert.DeserializeObject<Models.Internal.Tweet>(body);
                return MapFromInternalTweet(o);
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();

                return null;
            }
        }

        public static User GetSingleUser(this string body)
        {
            try
            {
                var o = JsonConvert.DeserializeObject<Models.Internal.User>(body);
                return MapUser(o);
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();

                return null;
            }
        }

        internal static DirectMessage GetSingleDirectMessage(this string body)
        {
            try
            {
                var o = JsonConvert.DeserializeObject<DM>(body);
                return MapFromInternal(o);
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();

                return null;
            }
        }

        private static Tweet MapFromInternalTweet(Models.Internal.Tweet o)
        {
            Tweet tweet;

            if (o.retweeted_status != null)
            {
                var status = o.retweeted_status;

                tweet = new Tweet
                            {
                                Id = status.id_str,
                                Text = Decode(status.text),
                                User = MapUser(status.user),
                                Time = status.created_at.ToDateTimeOffset(),
                                RetweetedBy = MapUser(o.user)
                            };
            }
            else
            {
                tweet = new Tweet
                {
                    Id = o.id_str,
                    Text = Decode(o.text),
                    User = MapUser(o.user),
                    Time = o.created_at.ToDateTimeOffset()
                };
            }

            if (o.entities != null)
            {
                if (o.entities.hashtags.HasAny())
                {
                    tweet.Hashtags = MapHashtags(o);
                }

                if (o.entities.user_mentions.HasAny())
                {
                    tweet.Mentions = MapMentions(o);
                }

                if (o.entities.media.HasAny())
                {
                    tweet.Media = MapMedia(o);
                }

                if (o.entities.urls.HasAny())
                {
                    tweet.Urls = MapUrls(o);
                }
            }

            return tweet;
        }

        private static string Decode(string text)
        {
            return WebUtility.HtmlDecode(text);
        }

        private static DirectMessage MapFromInternal(DM o)
        {
            var tweet = new DirectMessage
        {
            Id = o.id.ToString(),
            Text = o.text,
            User = o.sender.MapUser(),
            Recipient = o.recipient.MapUser(),
            Time = o.created_at.ParseDateTime()
        };
            return tweet;
        }

        private static Url[] MapUrls(Models.Internal.Tweet tweet)
        {
            return tweet.entities.urls.Select(u => new Url
                                                {
                                                    DisplayUrl = u.display_url,
                                                    ExpandedUrl = u.expanded_url,
                                                    OriginalUrl = u.url
                                                }).ToArray();
        }

        private static Media[] MapMedia(Models.Internal.Tweet tweet)
        {
            return tweet.entities.media.Select(m => new Media
                                                        {
                                                            DisplayUrl = m.display_url,
                                                            ExpandedUrl = m.expanded_url,
                                                            Id = m.id_str,
                                                            MediaUrl = m.media_url,
                                                            MediaUrlHttps = m.media_url_https
                                                        }).ToArray();
        }

        private static Mention[] MapMentions(Models.Internal.Tweet tweet)
        {
            return tweet.entities.user_mentions
                .Select(u => new Mention
                        {
                            Name = u.name,
                            ScreenName = u.screen_name,
                            Start = u.indices.First(),
                            End = u.indices.Last(),
                            Id = u.id_str
                        }
                ).ToArray();
        }

        private static Hashtag[] MapHashtags(Models.Internal.Tweet tweet)
        {
            return tweet.entities.hashtags
                .Select(h => new Hashtag
                {
                    Name = h.text,
                    Start = h.indices.First(),
                    End = h.indices.Last()
                }).ToArray();
        }

        public static IEnumerable<Tweet> FromSearchResponse(this string body)
        {
            var result = JsonConvert.DeserializeObject<SearchResponse>(body);

            return result.results.Select(c => new Tweet
            {
                Id = c.id_str,
                Text = c.text,
                User = MapUser(c),
                Time = c.created_at.ToDateTimeOffset()
            });
        }

        internal static User MapUser(this Models.Internal.User user)
        {
            if (user == null)
                return new User();

            return new User
            {
                Name = user.name,
                ScreenName = user.screen_name,
                Avatar = user.profile_image_url_https,
                Followers = user.followers_count,
                Friends = user.friends_count,
                IsProtected = user.is_protected,
                IsFollowedByMe = user.following.GetValueOrDefault(false)
            };
        }

        private static User MapUser(Result result)
        {
            return new User
            {
                ScreenName = result.from_user,
                Name = result.from_user_name,
                Avatar = result.profile_image_url_https
            };
        }

        public static IEnumerable<SavedSearch> GetSavedSearchList(this string body)
        {
            return Enumerable.Empty<SavedSearch>();
        }

        public static SavedSearch GetSavedSearch(this string body)
        {
            return new SavedSearch();
        }
    }
}


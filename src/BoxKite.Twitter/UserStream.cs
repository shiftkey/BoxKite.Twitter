using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using BoxKite.Twitter.Extensions;
using BoxKite.Twitter.Mappings;
using BoxKite.Twitter.Models;
using BoxKite.Twitter.Modules.Streaming;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tweet = BoxKite.Twitter.Models.Tweet;
using User = BoxKite.Twitter.Models.User;

namespace BoxKite.Twitter
{
    internal class UserStream : IUserStream
    {
        readonly Func<Task<HttpResponseMessage>> createOpenConnection;
        readonly Subject<Tweet> tweets = new Subject<Tweet>();
        readonly Subject<DirectMessage> directmessages = new Subject<DirectMessage>();
        readonly Subject<Event> events = new Subject<Event>();
        readonly Subject<IEnumerable<long>> friends = new Subject<IEnumerable<long>>();
        readonly TimeSpan initialDelay = TimeSpan.FromSeconds(20);

        bool isActive = true;
        TimeSpan delay = TimeSpan.FromSeconds(20);

        public UserStream(Func<Task<HttpResponseMessage>> createOpenConnection)
        {
            this.createOpenConnection = createOpenConnection;
        }

        public void Start()
        {
            Task.Factory.StartNew(ProcessMessages)
                .ContinueWith(HandleExceptionsIfRaised);
        }

        private void HandleExceptionsIfRaised(Task obj)
        {
            if (obj.Exception != null)
            {
                SendToAllSubscribers(obj.Exception);
            }

            if (obj.IsFaulted)
            {
                SendToAllSubscribers(new Exception("Stream is faulted"));
            }

            if (obj.IsCanceled)
            {
                SendToAllSubscribers(new Exception("Stream is cancelled"));
            }
        }

        private void SendToAllSubscribers(Exception exception)
        {
            tweets.OnError(exception);
            friends.OnError(exception);
            directmessages.OnError(exception);
        }

        public void Stop()
        {
            isActive = false;
        }

        public IObservable<Tweet> Tweets { get { return tweets; } }

        public IObservable<DirectMessage> DirectMessages { get { return directmessages; } }

        public IObservable<Event> Events { get { return events; } }

        public IObservable<IEnumerable<long>> Friends { get { return friends; } }

        private async void ProcessMessages()
        {
            var responseStream = await GetStream();
            while (isActive)
            {
                // reconnect if the stream was closed previously
                if (responseStream == null)
                {
                    await Task.Delay(delay);
                    responseStream = await GetStream();
                }

                string line;
                try
                {
                    line = responseStream.ReadLine();
                }
                catch (IOException)
                {
                    delay += initialDelay;
                    responseStream.Dispose();
                    responseStream = null;
                    line = "";
                }

                if (delay.TotalMinutes <= 2)
                {
                    // TODO: give up
                }

                if (String.IsNullOrEmpty(line)) continue;

                Debug.WriteLine(line);

                // we have a valid connection - clear delay
                delay = TimeSpan.Zero;

                var obj = JsonConvert.DeserializeObject<dynamic>(line);

                //https://dev.twitter.com/docs/streaming-apis/messages
                if (obj["friends"] != null)
                {
                    var friendIds = (JArray) obj["friends"];
                    var ids = friendIds.Values<long>();
                    SendFriendsMessage(ids);
                    continue;
                }

                if (obj["event"] != null)
                {
                    var eventValue = (JValue) obj["event"];
                    var eventText = eventValue.Value<string>();

                    var target = (JToken) obj["target"];
                    var source = (JToken) obj["source"];
                    var target_object = (JToken) obj["target_object"];
                    var created_at = (JToken) obj["target_object"];
                    var created_atText = eventValue.Value<string>();
                    var timestamp = created_atText.ToDateTimeOffset();

                    
                    // TODO: raise on appropriate feed
                    if (eventText.StartsWith("list"))
                    {
                        //return a ListEvent
                    } else
                    {
                        var e = new TweetEvent
                            {
                                EventName = eventText,
                                Source = MapStreamUser(target),
                                Target = MapStreamUser(source),
                                TargetObject = MapStreamTweet(target_object)
                            };

                        events.OnNext(e);
                    }
                    continue;
                }

                if (obj["direct_message"] != null)
                {
                    var streamdm = (JToken)obj["direct_message"];

                    directmessages.OnNext(MapStreamDm(streamdm));

                    continue;
                }

                if (obj["scrub_geo"] != null)
                {
                    continue;
                }

                if (obj["limit"] != null)
                {
                    continue;
                }
                
                if (obj["delete"] != null)
                {
                    continue;
                }
                    
                var tweet = line.GetSingle();
                if (tweet != null)
                {
                    tweets.OnNext(tweet);
                }
            }
        }

        private Tweet MapStreamTweet(dynamic t)
        {
            return new Tweet
            {
                Id = t.id.ToString(),
                Text = t.text,
                User = MapStreamUser(t.User),
                //Time = o.created_at.ParseDateTime()
            };
        }

        private DirectMessage MapStreamDm(JToken dm)
        {
            return new DirectMessage
            {
                Id = dm["id"].ToString(),
                Text = (string)dm["text"],
                User = MapStreamUser(dm["sender"]),
                Recipient = MapStreamUser(dm["recipient"]),
                //Time = o.created_at.ParseDateTime()
            };
        }

        private User MapStreamUser(dynamic user)
        {
            if (user == null)
                return null;

            return new User
                {
                    Name = user.name,
                    ScreenName = user.screen_name,
                    Avatar = user.profile_image_url_https,
                    Followers = user.followers_count,
                    Friends = user.friends_count,
                };
        }

        private async Task<StreamReader> GetStream()
        {
            var response = await createOpenConnection();
            var stream = await response.Content.ReadAsStreamAsync();

            var responseStream = new StreamReader(stream);
            return responseStream;
        }

        private void SendFriendsMessage(IEnumerable<long> obj)
        {
            friends.OnNext(obj);
        }

        public void Dispose()
        {
            isActive = false;

            friends.Dispose();
            tweets.Dispose();
        }
    }
}

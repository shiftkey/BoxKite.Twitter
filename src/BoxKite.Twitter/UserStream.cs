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

namespace BoxKite.Twitter
{
    internal class UserStream : IUserStream
    {
        readonly Func<Task<HttpResponseMessage>> createOpenConnection;
        readonly Subject<Tweet> tweets = new Subject<Tweet>();
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
        }

        public void Stop()
        {
            isActive = false;
        }

        public IObservable<Tweet> Tweets { get { return tweets; } }

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
                    
                    continue;
                }

                var tweet = line.GetSingle();
                if (tweet != null)
                {
                    tweets.OnNext(tweet);
                }
            }
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

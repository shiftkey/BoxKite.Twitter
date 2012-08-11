using System;
using System.Collections.Generic;
using BoxKite.Twitter.Models;

namespace BoxKite.Twitter.Modules.Streaming
{
    // TODO: expose stream of events
    
    public interface IUserStream : IDisposable
    {
        IObservable<Tweet> Tweets { get; }
        IObservable<IEnumerable<long>> Friends { get; }
        void Start();
        void Stop();
    }
}
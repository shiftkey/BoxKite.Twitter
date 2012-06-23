﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BoxKite.Twitter;
using BoxKite.Twitter.Extensions;
using BoxKite.Twitter.Modules.Streaming;

// ReSharper disable CheckNamespace
namespace BoxKite.Modules
// ReSharper restore CheckNamespace
{
    public static class UserStreamExtensions
    {
        public static IUserStream GetUserStream(this IUserSession session)
        {
            Func<Task<HttpResponseMessage>> startConnection =
                () =>
                {
                    var parameters = new SortedDictionary<string, string>();
                    var request = session.CreateGet(Api.Streaming("/2/user.json"), parameters);
                    var c = new HttpClient { Timeout = TimeSpan.FromDays(1) };
                    return c.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                };

            return new UserStream(startConnection);
        }
    }
}

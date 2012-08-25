using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoxKite.Twitter;
using BoxKite.Twitter.Authentication;
using BoxKite.Twitter.Models;

namespace ConsoleApplication1
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("OHAI");
            Task<TwitterCredentials> tcaw = SignIn("b8qsK6pFUPNZzdu5FxfxVg", "mYO5CysNHvFQ0pPO7y7Fwj7LY1KsLlxha794FXp7qM" );
            TwitterCredentials tc = tcaw.Result;
            UserSession us = GetSession(tc);
            Console.ReadLine();

        }

        public static async Task<TwitterCredentials> SignIn(string clientKey, string clientSecret)
        {
            return await TwitterAuthenticator.AuthenticateUser(clientKey, clientSecret, "oob");
         }

        public static UserSession GetSession(TwitterCredentials credentials)
        {
            return new UserSession(credentials);
        }

    }
}

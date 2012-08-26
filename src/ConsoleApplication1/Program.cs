using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoxKite.Modules;
using BoxKite.Twitter;
using BoxKite.Twitter.Authentication;
using BoxKite.Twitter.Models;
using BoxKite.Twitter.Extensions;
using BoxKite.Twitter.Modules;
using BoxKite.Twitter.Mappings;
using BoxKite.Twitter.Modules.Streaming;
using System.Reactive;

namespace ConsoleApplication1
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("OHAI");

            var ta = new TwitterAuthenticator("xxx", "xxx");
            ta.AuthenticateUser();
            Console.Write("pin: ");
            var pin = Console.ReadLine();
            if (ta.DelegateAuthentication(pin).Result)
            {
                TwitterCredentials tc = ta.GetUserCredentials();
                Console.WriteLine(tc.ScreenName + " is authorised to use BoxKite.Twitter. Yay");

                var session = new UserSession(tc);
                var stream = session.GetUserStream();
                stream.Tweets.Subscribe(t => Console.WriteLine("{0}: {1}", t.User.ScreenName, t.Text));
                stream.Start();
                while (true)
                {
                }
            }
            Console.WriteLine("All finished: ");
            Console.ReadLine();
        }
    }
}

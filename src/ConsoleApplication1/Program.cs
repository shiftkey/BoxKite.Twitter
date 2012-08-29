using BoxKite.Modules;
using BoxKite.Twitter;
using BoxKite.Twitter.Authentication;
using System;

namespace ConsoleApplication1
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("OHAI");

            var twitterauth = new TwitterAuthenticator("xxx", "xxx");
            twitterauth.StartAuthentication();
            Console.Write("pin: ");
            var pin = Console.ReadLine();
            var twittercredentials = twitterauth.ConfirmPin(pin).Result;
            if (twittercredentials.Valid)
            {
                Console.WriteLine(twittercredentials.ScreenName + " is authorised to use BoxKite.Twitter. Yay");

                var session = new UserSession(twittercredentials);
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

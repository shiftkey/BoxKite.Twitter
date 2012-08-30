using BoxKite.Modules;
using BoxKite.Twitter;
using BoxKite.Twitter.Authentication;
using System;

namespace BoxKite.Twitter45.Samples
{
    class ConsoleApp
    {

        static void Main(string[] args)
        {
            Console.WriteLine("OHAI");

            var twitterauth = new TwitterAuthenticator("xxx", "xxx");
            var authstartok = twitterauth.StartAuthentication();
            if (authstartok.Result)
            {
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
                        // will loop forever, but the Subscription above will print tweets out. hopefully :-)
                    }
                }
                Console.WriteLine("All finished: ");
            }
            else
            {
                Console.WriteLine("Authenticator could not start. Do you have the correct Client/Consumer IDs and secrets?");
            }
            Console.ReadLine();
        }
    }
}

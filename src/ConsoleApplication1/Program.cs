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

            var ta = new TwitterAuthenticator("b8qsK6pFUPNZzdu5FxfxVg", "mYO5CysNHvFQ0pPO7y7Fwj7LY1KsLlxha794FXp7qM");
            ;
            if (ta.AuthenticateUser().Result)
            {
                Console.Write("Enter PIN as shown in the browser window after Authorizing BoxKite: ");
                var pin = Console.ReadLine();
                if (ta.DelegateAuthentication(pin).Result)
                {
                    TwitterCredentials tc = ta.GetUserCredentials();
                    Console.WriteLine(tc.ScreenName + " is authorised to use BoxKite.Twitter. Yay");
                }
                Console.ReadLine();
            }
        }
    }
}

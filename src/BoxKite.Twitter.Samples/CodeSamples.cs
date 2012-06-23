using System;
using BoxKite.Twitter.Models;
using BoxKite.Twitter.Modules;

namespace BoxKite.Twitter.Samples
{
    public class CodeSamples
    {

        public void Start()
        {
            var session = new AnonymousSession();
            session.GetProfile("shiftkey")
                   .Subscribe(DisplayUser);

            session.SearchFor("twitter")
                   .Subscribe(DisplayResult);



        }

        private void DisplayUser(User user)
        {

        }

        private void DisplayResult(Tweet tweet)
        {
            
        }
    }
}

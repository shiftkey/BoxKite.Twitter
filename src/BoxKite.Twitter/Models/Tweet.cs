using System;
using BoxKite.Twitter.Extensions;

namespace BoxKite.Twitter.Models
{
    public class Tweet : Synchronizable
    {
        public string Text { get; set; }

        public DateTimeOffset Time { get; set; }

        public string FriendlyTime
        {
            get { return Time.ToFriendlyText(); }
        }

        public string Id { get; set; }

        public User User { get; set; }

        public User RetweetedBy { get; set; }

        public bool Favourited { get; set; }

        private Hashtag[] hashtags = new Hashtag[0];
        public Hashtag[] Hashtags
        {
            get { return hashtags; }
            set { SetProperty(ref hashtags, value); }
        }

        private Mention[] mentions = new Mention[0];
        public Mention[] Mentions
        {
            get { return mentions; }
            set { SetProperty(ref mentions, value); }
        }

        private Media[] media = new Media[0];
        public Media[] Media
        {
            get { return media; }
            set { SetProperty(ref media, value); }
        }

        private Url[] url = new Url[0];
        public Url[] Urls
        {
            get { return url; }
            set { SetProperty(ref url, value); }
        }

        public override void Refresh()
        {
            OnPropertyChanged("FriendlyTime");
        }
    }
}

using System;
using BoxKite.Twitter.Extensions;

namespace BoxKite.Twitter.Models
{
    public class Tweet : Synchronizable
    {
        private string text;
        public string Text
        {
            get { return text; }
            set { SetProperty(ref text, value); }
        }

        private DateTimeOffset date;
        public DateTimeOffset Time
        {
            get { return date; }
            set { SetProperty(ref date, value); }
        }

        public string FriendlyTime
        {
            get { return Time.ToFriendlyText(); }
        }

        private string id;
        public string Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        private User user;
        public User User
        {
            get { return user; }
            set { SetProperty(ref user, value); }
        }

        private User retweetedBy;
        public User RetweetedBy
        {
            get { return retweetedBy; }
            set { SetProperty(ref retweetedBy, value); }
        }

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

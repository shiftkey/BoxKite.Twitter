namespace BoxKite.Twitter.Models
{
    public class User : Synchronizable
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private string avatar;
        public string Avatar
        {
            get { return avatar; }
            set { SetProperty(ref avatar, value); }
        }

        private int followers;
        public int Followers
        {
            get { return followers; }
            set { SetProperty(ref followers, value); }
        }

        private int friends;
        public int Friends
        {
            get { return friends; }
            set { SetProperty(ref friends, value); }
        }

        private bool isProtected;
        public bool IsProtected
        {
            get { return isProtected; }
            set { SetProperty(ref isProtected, value); }
        }

        private bool isFollowedByMe;
        public bool IsFollowedByMe
        {
            get { return isFollowedByMe; }
            set { SetProperty(ref isFollowedByMe, value); }
        }

        private bool isFollowingMe;
        public bool IsFollowingMe
        {
            get { return isFollowingMe; }
            set { SetProperty(ref isFollowingMe, value); }
        }

        private string screenName;
        public string ScreenName
        {
            get { return screenName; }
            set { SetProperty(ref screenName, value); }
        }

    }
}
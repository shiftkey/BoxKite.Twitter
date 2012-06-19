namespace BoxKite.Twitter.Models
{
    public class Url : PropertyChangedBase
    {
        private string displayUrl;
        public string DisplayUrl
        {
            get { return displayUrl; }
            set { SetProperty(ref displayUrl, value); }
        }

        private string expandedUrl;
        public string ExpandedUrl
        {
            get { return expandedUrl; }
            set { SetProperty(ref expandedUrl, value); }
        }

        private string url;
        public string OriginalUrl
        {
            get { return url; }
            set { SetProperty(ref url, value); }
        }
    }
}
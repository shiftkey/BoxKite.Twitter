using System.Collections.Generic;

namespace BoxKite.Twitter.Models.Internal
{
    internal class Medium
    {
        public long id { get; set; }
        public string id_str { get; set; }
        public string media_url { get; set; }
        public string media_url_https { get; set; }
        public string url { get; set; }
        public string display_url { get; set; }
        public string expanded_url { get; set; }
        public Sizes sizes { get; set; }
        public string type { get; set; }
        public List<int> indices { get; set; }
    }
}
using System.Collections.Generic;

namespace BoxKite.Twitter.Models.Internal
{
    internal class Url
    {
        public string url { get; set; }
        public string display_url { get; set; }
        public string expanded_url { get; set; }
        public List<int> indices { get; set; }
    }
}
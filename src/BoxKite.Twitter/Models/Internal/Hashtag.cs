using System.Collections.Generic;

namespace BoxKite.Twitter.Models.Internal
{
    public class Hashtag
    {
        public string text { get; set; }
        public List<int> indices { get; set; }
    }
}
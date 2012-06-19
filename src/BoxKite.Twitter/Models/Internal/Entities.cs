using System.Collections.Generic;

namespace BoxKite.Twitter.Models.Internal
{
    internal class Entities
    {
        public List<Medium> media { get; set; }
        public List<Hashtag> hashtags { get; set; }
        public List<Url> urls { get; set; }
        public List<UserMention> user_mentions { get; set; }
    }
}
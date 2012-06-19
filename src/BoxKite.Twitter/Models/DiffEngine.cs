using System.Collections.Generic;

namespace BoxKite.Twitter.Models
{
    public class DiffEngine
    {
        public int NewFollowersCount { get; set; }
        public IEnumerable<long> NewFollowers { get; set; }

        public int LostFollowersCount { get; set; }
        public IEnumerable<long> LostFollowers { get; set; }

        public int NewFriendsCount { get; set; }
        public IEnumerable<long> NewFriends { get; set; }

        public IEnumerable<long> LostFriends { get; set; }
    }
}
using System.Collections.Generic;

namespace BoxKite.Twitter.Models.Internal
{
    public class Ids
    {
            public int previous_cursor { get; set; }
            public List<long> ids { get; set; }
            public string previous_cursor_str { get; set; }
            public int next_cursor { get; set; }
            public string next_cursor_str { get; set; }
     
    }
}

using System.Collections.Generic;

namespace BoxKite.Twitter.Models.Internal
{
    internal class Geo
    {
        public string type { get; set; }
        public List<double> coordinates { get; set; }
    }
}
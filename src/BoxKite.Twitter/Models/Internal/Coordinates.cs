using System.Collections.Generic;

namespace BoxKite.Twitter.Models.Internal
{
    internal class Coordinates
    {
        public string type { get; set; }
        public List<double> coordinates { get; set; }
    }
}
namespace BoxKite.Twitter.Models
{
    public class Mention : PropertyChangedBase
    {
        public string Name { get; set; }

        public int Start { get; set; }

        public string ScreenName { get; set; }

        public int End { get; set; }

        public string Id { get; set; }
    }
}
using System.ComponentModel;

namespace BoxKite.Twitter.Models
{
    public class Hashtag : INotifyPropertyChanged
    {
        public string Name { get; set; }

        public int Start { get; set; }

        public int End { get; set; }
#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
    }
}
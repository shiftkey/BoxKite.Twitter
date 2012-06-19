namespace BoxKite.Twitter.Models
{
    public class DirectMessage : Tweet
    {
        public User Recipient { get; set; }
    }
}
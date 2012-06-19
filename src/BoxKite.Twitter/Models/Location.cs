namespace BoxKite.Twitter.Models
{
    public class Location
    {
        public Location()
        {

        }

        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }
        
        public bool IsDefined
        {
            get { return Longitude.HasValue && Latitude.HasValue; }
        }

        public void Clear()
        {
            Latitude = null;
            Longitude = null;
        }
    }
}
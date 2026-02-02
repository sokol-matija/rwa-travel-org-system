namespace WebAPI.Models
{
    public class TripGuide
    {
        public int TripId { get; set; }
        public int GuideId { get; set; }

        // Navigation properties
        public Trip Trip { get; set; } = null!;
        public Guide Guide { get; set; } = null!;
    }
}

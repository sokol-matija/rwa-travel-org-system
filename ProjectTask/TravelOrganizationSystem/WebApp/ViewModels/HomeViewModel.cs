using WebApp.Models;

namespace WebApp.ViewModels
{
    public class HomeViewModel
    {
        public List<DestinationModel> FeaturedDestinations { get; set; } = new List<DestinationModel>();
        public string? ErrorMessage { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using WebApp.Models;

namespace WebApp.ViewModels
{
    public class DestinationIndexViewModel
    {
        public List<DestinationModel> Destinations { get; set; } = new List<DestinationModel>();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 6;
        public int TotalDestinations { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
        public string? ErrorMessage { get; set; }
    }

    public class DestinationDetailsViewModel
    {
        public DestinationModel Destination { get; set; } = default!;
        public List<TripModel> Trips { get; set; } = new List<TripModel>();
        public int TripsCount { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class CreateDestinationViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        [Display(Name = "Destination Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Country is required")]
        [StringLength(100, ErrorMessage = "Country cannot be longer than 100 characters")]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot be longer than 100 characters")]
        public string City { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Image URL cannot be longer than 500 characters")]
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Climate")]
        [StringLength(200, ErrorMessage = "Climate description cannot exceed 200 characters")]
        public string? Climate { get; set; }

        [Display(Name = "Best Time to Visit")]
        [StringLength(200, ErrorMessage = "Best time to visit description cannot exceed 200 characters")]
        public string? BestTimeToVisit { get; set; }
    }

    public class EditDestinationViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        [Display(Name = "Destination Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Country is required")]
        [StringLength(100, ErrorMessage = "Country cannot be longer than 100 characters")]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot be longer than 100 characters")]
        public string City { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Image URL cannot be longer than 500 characters")]
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Climate")]
        [StringLength(200, ErrorMessage = "Climate description cannot exceed 200 characters")]
        public string? Climate { get; set; }

        [Display(Name = "Best Time to Visit")]
        [StringLength(200, ErrorMessage = "Best time to visit description cannot exceed 200 characters")]
        public string? BestTimeToVisit { get; set; }

        [Display(Name = "Tagline")]
        [StringLength(200, ErrorMessage = "Tagline cannot exceed 200 characters")]
        public string? Tagline { get; set; }
    }
}

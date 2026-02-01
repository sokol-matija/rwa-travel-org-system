using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Models;

namespace WebApp.ViewModels
{
    public class TripIndexViewModel
    {
        public List<TripModel> Trips { get; set; } = new List<TripModel>();
        public SelectList Destinations { get; set; } = default!;
        public int? DestinationId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalTrips { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
        public string? ErrorMessage { get; set; }
    }

    public class TripDetailsViewModel
    {
        public TripModel? Trip { get; set; }
        public DestinationModel? Destination { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class TripBookViewModel
    {
        public TripModel? Trip { get; set; }
        public string? ErrorMessage { get; set; }
        public int TripId { get; set; }

        [Range(1, 10, ErrorMessage = "Number of participants must be between 1 and 10")]
        public int NumberOfParticipants { get; set; } = 1;
    }

    public class MyBookingsViewModel
    {
        public List<TripRegistrationModel> Bookings { get; set; } = new List<TripRegistrationModel>();
        public string? ErrorMessage { get; set; }
    }

    public class CreateTripModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "End date is required")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Price (USD)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 500, ErrorMessage = "Capacity must be between 1 and 500")]
        [Display(Name = "Maximum Participants")]
        public int Capacity { get; set; } = 10;

        [Display(Name = "Image URL")]
        [StringLength(500, ErrorMessage = "Image URL cannot be longer than 500 characters")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Destination is required")]
        [Display(Name = "Destination")]
        public int DestinationId { get; set; }
    }

    public class CreateTripViewModel
    {
        public CreateTripModel Trip { get; set; } = new CreateTripModel();
        public List<SelectListItem> Destinations { get; set; } = new List<SelectListItem>();
    }

    public class EditTripModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Price (USD)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 500, ErrorMessage = "Capacity must be between 1 and 500")]
        [Display(Name = "Maximum Participants")]
        public int Capacity { get; set; }

        [Display(Name = "Image URL")]
        [StringLength(500, ErrorMessage = "Image URL cannot be longer than 500 characters")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Destination is required")]
        [Display(Name = "Destination")]
        public int DestinationId { get; set; }
    }

    public class EditTripViewModel
    {
        public EditTripModel Trip { get; set; } = new EditTripModel();
        public List<SelectListItem> Destinations { get; set; } = new List<SelectListItem>();
    }
}

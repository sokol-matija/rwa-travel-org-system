using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Models;
using WebApp.Services;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [Authorize]
    public class TripsController : Controller
    {
        private readonly ITripService _tripService;
        private readonly IDestinationService _destinationService;
        private readonly ILogger<TripsController> _logger;

        public TripsController(
            ITripService tripService,
            IDestinationService destinationService,
            ILogger<TripsController> logger)
        {
            _tripService = tripService;
            _destinationService = destinationService;
            _logger = logger;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(int? DestinationId, int page = 1)
        {
            var vm = new TripIndexViewModel
            {
                DestinationId = DestinationId,
                Page = page < 1 ? 1 : page
            };

            try
            {
                _logger.LogInformation("Loading trips page with destination filter: {DestinationId}, Page: {Page}", DestinationId, page);

                var destinations = await _destinationService.GetAllDestinationsAsync();
                vm.Destinations = new SelectList(destinations, nameof(DestinationModel.Id), nameof(DestinationModel.Name));

                var (trips, totalCount) = await _tripService.GetTripsAsync(vm.Page, vm.PageSize, DestinationId);

                vm.Trips = trips;
                vm.TotalTrips = totalCount;
                vm.TotalPages = (int)Math.Ceiling((double)totalCount / vm.PageSize);

                foreach (var trip in vm.Trips)
                {
                    var destination = destinations.FirstOrDefault(d => d.Id == trip.DestinationId);
                    if (destination != null && string.IsNullOrEmpty(trip.DestinationName))
                    {
                        trip.DestinationName = destination.Name;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading trips");
                vm.ErrorMessage = $"Error loading trips: {ex.Message}";
            }

            return View(vm);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id, int? DestinationId, int page = 1)
        {
            try
            {
                _logger.LogInformation("Deleting trip: {Id}", id);
                var result = await _tripService.DeleteTripAsync(id);

                if (result)
                {
                    TempData["SuccessMessage"] = "Trip deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete the trip. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting trip: {Id}", id);
                TempData["ErrorMessage"] = $"An error occurred while deleting the trip: {ex.Message}";
            }

            return RedirectToAction("Index", new { DestinationId, page });
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var vm = new TripDetailsViewModel();

            try
            {
                vm.Trip = await _tripService.GetTripByIdAsync(id.Value);
                if (vm.Trip == null) return NotFound();

                vm.Destination = await _destinationService.GetDestinationByIdAsync(vm.Trip.DestinationId);
                if (vm.Destination != null)
                {
                    vm.Trip.DestinationName = vm.Destination.Name;
                }
            }
            catch (Exception ex)
            {
                vm.ErrorMessage = $"Error loading trip details: {ex.Message}";
            }

            return View(vm);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new CreateTripViewModel();
            await LoadDestinationsForCreate(vm);
            return View(vm);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateTripViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadDestinationsForCreate(vm);
                return View(vm);
            }

            if (vm.Trip.StartDate >= vm.Trip.EndDate)
            {
                ModelState.AddModelError("Trip.EndDate", "End date must be after start date.");
                await LoadDestinationsForCreate(vm);
                return View(vm);
            }

            if (vm.Trip.StartDate < DateTime.Today)
            {
                ModelState.AddModelError("Trip.StartDate", "Start date cannot be in the past.");
                await LoadDestinationsForCreate(vm);
                return View(vm);
            }

            try
            {
                // If no image provided, use the destination's image
                var imageUrl = vm.Trip.ImageUrl;
                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    var destination = await _destinationService.GetDestinationByIdAsync(vm.Trip.DestinationId);
                    if (destination != null)
                    {
                        imageUrl = destination.ImageUrl;
                    }
                }

                var trip = new TripModel
                {
                    Title = vm.Trip.Title,
                    Description = vm.Trip.Description,
                    StartDate = vm.Trip.StartDate,
                    EndDate = vm.Trip.EndDate,
                    Price = vm.Trip.Price,
                    Capacity = vm.Trip.Capacity,
                    ImageUrl = imageUrl,
                    DestinationId = vm.Trip.DestinationId
                };

                var result = await _tripService.CreateTripAsync(trip);
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Trip created successfully!";
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", "Failed to create trip. Please try again.");
                await LoadDestinationsForCreate(vm);
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating trip: {Title}", vm.Trip.Title);
                ModelState.AddModelError("", $"An error occurred while creating the trip: {ex.Message}");
                await LoadDestinationsForCreate(vm);
                return View(vm);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var trip = await _tripService.GetTripByIdAsync(id);
                if (trip == null) return NotFound();

                var vm = new EditTripViewModel
                {
                    Trip = new EditTripModel
                    {
                        Id = trip.Id,
                        Title = trip.Title,
                        Description = trip.Description,
                        StartDate = trip.StartDate,
                        EndDate = trip.EndDate,
                        Price = trip.Price,
                        Capacity = trip.Capacity,
                        ImageUrl = trip.ImageUrl,
                        DestinationId = trip.DestinationId
                    }
                };

                await LoadDestinationsForEdit(vm);
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trip for edit: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving the trip.";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(EditTripViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadDestinationsForEdit(vm);
                return View(vm);
            }

            if (vm.Trip.StartDate >= vm.Trip.EndDate)
            {
                ModelState.AddModelError("Trip.EndDate", "End date must be after start date.");
                await LoadDestinationsForEdit(vm);
                return View(vm);
            }

            if (vm.Trip.StartDate < DateTime.Today)
            {
                ModelState.AddModelError("Trip.StartDate", "Start date cannot be in the past.");
                await LoadDestinationsForEdit(vm);
                return View(vm);
            }

            try
            {
                // If no image provided, use the destination's image
                var imageUrl = vm.Trip.ImageUrl;
                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    var destination = await _destinationService.GetDestinationByIdAsync(vm.Trip.DestinationId);
                    if (destination != null)
                    {
                        imageUrl = destination.ImageUrl;
                    }
                }

                var tripModel = new TripModel
                {
                    Id = vm.Trip.Id,
                    Title = vm.Trip.Title,
                    Description = vm.Trip.Description,
                    StartDate = vm.Trip.StartDate,
                    EndDate = vm.Trip.EndDate,
                    Price = vm.Trip.Price,
                    Capacity = vm.Trip.Capacity,
                    ImageUrl = imageUrl,
                    DestinationId = vm.Trip.DestinationId
                };

                var result = await _tripService.UpdateTripAsync(vm.Trip.Id, tripModel);

                if (result != null)
                {
                    TempData["SuccessMessage"] = "Trip updated successfully!";
                    return RedirectToAction("Details", new { id = vm.Trip.Id });
                }

                ModelState.AddModelError("", "Failed to update trip. Please try again.");
                await LoadDestinationsForEdit(vm);
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trip: {Id}", vm.Trip.Id);
                ModelState.AddModelError("", "An error occurred while updating the trip. Please try again.");
                await LoadDestinationsForEdit(vm);
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Book(int id)
        {
            var vm = new TripBookViewModel { TripId = id };

            try
            {
                var trip = await _tripService.GetTripByIdAsync(id);
                if (trip == null) return NotFound();

                if (trip.CurrentBookings >= trip.Capacity)
                {
                    vm.ErrorMessage = "This trip is fully booked. Please select another trip.";
                    return View(vm);
                }

                vm.Trip = trip;

                if (trip.DestinationId > 0 && string.IsNullOrEmpty(trip.DestinationName))
                {
                    var destination = await _destinationService.GetDestinationByIdAsync(trip.DestinationId);
                    if (destination != null)
                    {
                        trip.DestinationName = destination.Name;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading booking page for trip: {TripId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the booking page. Please try again.";
                return RedirectToAction("Index");
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(TripBookViewModel vm)
        {
            ModelState.Remove("Trip");
            if (!ModelState.IsValid)
            {
                var trip = await _tripService.GetTripByIdAsync(vm.TripId);
                if (trip != null) vm.Trip = trip;
                return View(vm);
            }

            try
            {
                var result = await _tripService.BookTripAsync(vm.TripId, vm.NumberOfParticipants);

                if (result)
                {
                    TempData["SuccessMessage"] = "Your trip has been booked successfully!";
                    return RedirectToAction("MyBookings");
                }

                vm.ErrorMessage = "Failed to book the trip. Please try again.";
                var tripData = await _tripService.GetTripByIdAsync(vm.TripId);
                if (tripData != null) vm.Trip = tripData;
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error booking trip: {TripId}", vm.TripId);
                vm.ErrorMessage = $"An error occurred while booking the trip: {ex.Message}";
                var tripData = await _tripService.GetTripByIdAsync(vm.TripId);
                if (tripData != null) vm.Trip = tripData;
                return View(vm);
            }
        }

        public async Task<IActionResult> MyBookings()
        {
            var vm = new MyBookingsViewModel();

            try
            {
                vm.Bookings = await _tripService.GetUserTripsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user's bookings");
                vm.ErrorMessage = "An error occurred while loading your bookings. Please try again later.";
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int id)
        {
            try
            {
                var result = await _tripService.CancelBookingAsync(id);

                if (result)
                {
                    TempData["SuccessMessage"] = "Your booking has been successfully cancelled.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to cancel your booking. Please try again later.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking {BookingId}", id);
                TempData["ErrorMessage"] = "An error occurred while cancelling your booking. Please try again later.";
            }

            return RedirectToAction("MyBookings");
        }

        private async Task LoadDestinationsForCreate(CreateTripViewModel vm)
        {
            try
            {
                var destinations = await _destinationService.GetAllDestinationsAsync();
                vm.Destinations = destinations
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = $"{d.Name} - {d.City}, {d.Country}"
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading destinations");
                vm.Destinations = new List<SelectListItem>();
            }
        }

        private async Task LoadDestinationsForEdit(EditTripViewModel vm)
        {
            try
            {
                var destinations = await _destinationService.GetAllDestinationsAsync();
                vm.Destinations = destinations
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = $"{d.Name} - {d.City}, {d.Country}"
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading destinations");
                vm.Destinations = new List<SelectListItem>();
            }
        }
    }
}

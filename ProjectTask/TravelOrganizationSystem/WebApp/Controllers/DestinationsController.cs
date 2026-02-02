using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Services;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [Authorize]
    public class DestinationsController : Controller
    {
        private readonly IDestinationService _destinationService;
        private readonly ITripService _tripService;
        private readonly IUnsplashService _unsplashService;
        private readonly ILogger<DestinationsController> _logger;

        public DestinationsController(
            IDestinationService destinationService,
            ITripService tripService,
            IUnsplashService unsplashService,
            ILogger<DestinationsController> logger)
        {
            _destinationService = destinationService;
            _tripService = tripService;
            _unsplashService = unsplashService;
            _logger = logger;
        }

        /// <summary>
        /// Display all destinations with pagination
        /// </summary>
        [AllowAnonymous]
        public async Task<IActionResult> Index(int page = 1)
        {
            var vm = new DestinationIndexViewModel { Page = page };

            try
            {
                var allDestinations = await _destinationService.GetAllDestinationsAsync();

                // Get images for destinations that don't have one
                foreach (var destination in allDestinations.Where(d => string.IsNullOrEmpty(d.ImageUrl)))
                {
                    var imageUrl = await _unsplashService.GetRandomImageUrlAsync($"{destination.City} {destination.Country}");
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        destination.ImageUrl = imageUrl;
                    }
                }

                // Apply pagination
                vm.TotalDestinations = allDestinations.Count;
                vm.TotalPages = (int)Math.Ceiling((double)allDestinations.Count / vm.PageSize);
                vm.Destinations = allDestinations
                    .Skip((page - 1) * vm.PageSize)
                    .Take(vm.PageSize)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading destinations");
                vm.ErrorMessage = "Failed to load destinations. Please try again later.";
            }

            return View(vm);
        }

        /// <summary>
        /// Display destination details
        /// </summary>
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vm = new DestinationDetailsViewModel();

            try
            {
                var destination = await _destinationService.GetDestinationByIdAsync(id.Value);

                if (destination == null)
                {
                    _logger.LogWarning("Destination not found: {Id}", id);
                    return NotFound();
                }

                vm.Destination = destination;

                // Get trips for this destination
                var trips = await _tripService.GetTripsByDestinationAsync(id.Value);
                vm.Trips = trips ?? new List<TripModel>();
                vm.TripsCount = vm.Trips.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving destination: {Id}", id);
                vm.ErrorMessage = "An error occurred while retrieving the destination details.";
            }

            return View(vm);
        }

        /// <summary>
        /// Display create destination form (admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateDestinationViewModel());
        }

        /// <summary>
        /// Handle create destination form submission (admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDestinationViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                var destination = new DestinationModel
                {
                    Name = vm.Name,
                    Description = vm.Description ?? string.Empty,
                    Country = vm.Country,
                    City = vm.City,
                    ImageUrl = vm.ImageUrl,
                    Climate = vm.Climate,
                    BestTimeToVisit = vm.BestTimeToVisit
                };

                var result = await _destinationService.CreateDestinationAsync(destination);
                if (result != null)
                {
                    _logger.LogInformation("Successfully created destination: {Name}", destination.Name);
                    TempData["SuccessMessage"] = "Destination created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    _logger.LogWarning("Failed to create destination: {Name}", destination.Name);
                    ModelState.AddModelError("", "Failed to create destination. Please try again.");
                    return View(vm);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating destination: {Name}", vm.Name);
                ModelState.AddModelError("", "An error occurred while creating the destination. Please try again.");
                return View(vm);
            }
        }

        /// <summary>
        /// Display edit destination form (admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var destination = await _destinationService.GetDestinationByIdAsync(id.Value);

                if (destination == null)
                {
                    _logger.LogWarning("Destination not found for edit: {Id}", id);
                    return NotFound();
                }

                var vm = new EditDestinationViewModel
                {
                    Id = destination.Id,
                    Name = destination.Name,
                    Description = destination.Description,
                    Country = destination.Country,
                    City = destination.City,
                    ImageUrl = destination.ImageUrl,
                    Climate = destination.Climate,
                    BestTimeToVisit = destination.BestTimeToVisit,
                    Tagline = destination.Tagline
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving destination for edit: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving the destination.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Handle edit destination form submission (admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditDestinationViewModel vm)
        {
            if (id != vm.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                var destination = new DestinationModel
                {
                    Id = vm.Id,
                    Name = vm.Name,
                    Description = vm.Description,
                    Country = vm.Country,
                    City = vm.City,
                    ImageUrl = vm.ImageUrl,
                    Climate = vm.Climate,
                    BestTimeToVisit = vm.BestTimeToVisit,
                    Tagline = vm.Tagline
                };

                await _destinationService.UpdateDestinationAsync(vm.Id, destination);

                _logger.LogInformation("Destination updated: {Id}", vm.Id);
                TempData["SuccessMessage"] = "Destination updated successfully!";

                return RedirectToAction(nameof(Details), new { id = vm.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating destination: {Id}", vm.Id);
                ModelState.AddModelError("", "An error occurred while updating the destination. Please try again.");
                return View(vm);
            }
        }

        /// <summary>
        /// Handle delete destination (admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _destinationService.DeleteDestinationAsync(id);
                _logger.LogInformation("Destination deleted successfully: {Id}", id);
                TempData["SuccessMessage"] = "Destination deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting destination: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the destination.";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Handle update destination image (admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateImage(int id, string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                TempData["ErrorMessage"] = "Image URL is required.";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                var destination = await _destinationService.GetDestinationByIdAsync(id);

                if (destination == null)
                {
                    return NotFound();
                }

                destination.ImageUrl = imageUrl;
                await _destinationService.UpdateDestinationAsync(id, destination);

                _logger.LogInformation("Destination image updated: {Id}", id);
                TempData["SuccessMessage"] = "Destination image successfully updated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating destination image: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while updating the destination image.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}

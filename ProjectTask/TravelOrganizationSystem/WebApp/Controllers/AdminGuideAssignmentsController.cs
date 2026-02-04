using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Models;
using WebApp.Services;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminGuideAssignmentsController : Controller
    {
        private readonly ITripService _tripService;
        private readonly IGuideService _guideService;
        private readonly ILogger<AdminGuideAssignmentsController> _logger;

        public AdminGuideAssignmentsController(
            ITripService tripService,
            IGuideService guideService,
            ILogger<AdminGuideAssignmentsController> logger)
        {
            _tripService = tripService;
            _guideService = guideService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = new AdminGuideAssignmentsIndexViewModel();

            try
            {
                _logger.LogInformation("Loading guide assignments page");

                await LoadDataAsync(vm);

                _logger.LogInformation("Loaded {TripCount} trips and {GuideCount} guides",
                    vm.Trips.Count, vm.Guides.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading guide assignments page");
                vm.ErrorMessage = "Unable to load guide assignments. Please try again later.";
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int tripId, int guideId)
        {
            try
            {
                _logger.LogInformation("Admin attempting to assign guide {GuideId} to trip {TripId}", guideId, tripId);

                if (tripId <= 0 || guideId <= 0)
                {
                    return Json(new { success = false, message = "Invalid trip or guide ID." });
                }

                var success = await _tripService.AssignGuideToTripAsync(tripId, guideId);

                if (success)
                {
                    _logger.LogInformation("Successfully assigned guide {GuideId} to trip {TripId}", guideId, tripId);

                    var guide = await _guideService.GetGuideByIdAsync(guideId);
                    var trip = await _tripService.GetTripByIdAsync(tripId);

                    var message = $"Successfully assigned {guide?.FullName ?? "guide"} to {trip?.Title ?? "trip"}.";

                    return Json(new { success = true, message });
                }
                else
                {
                    _logger.LogWarning("Failed to assign guide {GuideId} to trip {TripId}", guideId, tripId);
                    return Json(new { success = false, message = "Failed to assign guide. The guide may already be assigned to this trip." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while assigning guide {GuideId} to trip {TripId}", guideId, tripId);
                return Json(new { success = false, message = "An error occurred while assigning the guide." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int tripId, int guideId)
        {
            try
            {
                _logger.LogInformation("Admin attempting to remove guide {GuideId} from trip {TripId}", guideId, tripId);

                if (tripId <= 0 || guideId <= 0)
                {
                    return Json(new { success = false, message = "Invalid trip or guide ID." });
                }

                var success = await _tripService.RemoveGuideFromTripAsync(tripId, guideId);

                if (success)
                {
                    _logger.LogInformation("Successfully removed guide {GuideId} from trip {TripId}", guideId, tripId);

                    var guide = await _guideService.GetGuideByIdAsync(guideId);
                    var trip = await _tripService.GetTripByIdAsync(tripId);

                    var message = $"Successfully removed {guide?.FullName ?? "guide"} from {trip?.Title ?? "trip"}.";

                    return Json(new { success = true, message });
                }
                else
                {
                    _logger.LogWarning("Failed to remove guide {GuideId} from trip {TripId}", guideId, tripId);
                    return Json(new { success = false, message = "Failed to remove guide. The guide may not be assigned to this trip." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while removing guide {GuideId} from trip {TripId}", guideId, tripId);
                return Json(new { success = false, message = "An error occurred while removing the guide." });
            }
        }

        private async Task LoadDataAsync(AdminGuideAssignmentsIndexViewModel vm)
        {
            vm.Trips = await _tripService.GetAllTripsAsync();
            vm.Guides = (await _guideService.GetAllGuidesAsync()).ToList();

            vm.TripSelectList = new SelectList(vm.Trips, nameof(TripModel.Id), nameof(TripModel.Title));
            vm.GuideSelectList = new SelectList(vm.Guides, nameof(GuideModel.Id), nameof(GuideModel.FullName));
        }
    }
}

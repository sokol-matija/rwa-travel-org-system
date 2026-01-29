using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Services;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    /// <summary>
    /// MVC Controller for managing travel guides (Admin only)
    /// Provides CRUD operations and AJAX functionality
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminGuidesController : Controller
    {
        private readonly IGuideService _guideService;
        private readonly ITripService _tripService;
        private readonly ILogger<AdminGuidesController> _logger;

        public AdminGuidesController(
            IGuideService guideService,
            ITripService tripService,
            ILogger<AdminGuidesController> logger)
        {
            _guideService = guideService;
            _tripService = tripService;
            _logger = logger;
        }

        /// <summary>
        /// Display guides list with optional search filtering
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string? searchFilter)
        {
            var vm = new AdminGuidesIndexViewModel
            {
                SearchFilter = searchFilter
            };

            try
            {
                _logger.LogInformation("Loading admin guides page with search filter: {SearchFilter}", searchFilter ?? "none");

                var allGuides = await _guideService.GetAllGuidesAsync();

                if (!string.IsNullOrWhiteSpace(searchFilter))
                {
                    var searchTerm = searchFilter.ToLowerInvariant();
                    vm.Guides = allGuides.Where(g =>
                        (!string.IsNullOrEmpty(g.FirstName) && g.FirstName.ToLowerInvariant().Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(g.LastName) && g.LastName.ToLowerInvariant().Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(g.Email) && g.Email.ToLowerInvariant().Contains(searchTerm)) ||
                        g.FullName.ToLowerInvariant().Contains(searchTerm)
                    ).ToList();

                    _logger.LogInformation("Applied search filter '{SearchFilter}', found {FilteredCount} out of {TotalCount} guides",
                        searchFilter, vm.Guides.Count(), allGuides.Count());
                }
                else
                {
                    vm.Guides = allGuides;
                    _logger.LogInformation("Loaded {Count} guides without filter", vm.Guides.Count());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading guides for admin page");
                vm.ErrorMessage = "Unable to load guides. Please try again later.";
                vm.Guides = new List<GuideModel>();
            }

            return View(vm);
        }

        /// <summary>
        /// AJAX handler for searching guides
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm = "")
        {
            try
            {
                _logger.LogInformation("AJAX search request for guides with term: '{SearchTerm}'", searchTerm);

                var allGuides = await _guideService.GetAllGuidesAsync();

                IEnumerable<GuideModel> filteredGuides;

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLowerInvariant();
                    filteredGuides = allGuides.Where(g =>
                        (!string.IsNullOrEmpty(g.FirstName) && g.FirstName.ToLowerInvariant().Contains(searchLower)) ||
                        (!string.IsNullOrEmpty(g.LastName) && g.LastName.ToLowerInvariant().Contains(searchLower)) ||
                        (!string.IsNullOrEmpty(g.Email) && g.Email.ToLowerInvariant().Contains(searchLower)) ||
                        g.FullName.ToLowerInvariant().Contains(searchLower)
                    );
                }
                else
                {
                    filteredGuides = allGuides;
                }

                var guideData = filteredGuides.Select(g => new
                {
                    id = g.Id,
                    fullName = g.FullName,
                    email = g.Email,
                    phoneNumber = g.PhoneNumber,
                    bio = g.Bio,
                    photoUrl = g.PhotoUrl,
                    yearsExperience = g.YearsExperience,
                    hasPhoto = !string.IsNullOrEmpty(g.PhotoUrl)
                });

                _logger.LogInformation("AJAX search returned {Count} guides for term: '{SearchTerm}'",
                    guideData.Count(), searchTerm);

                return Json(new { success = true, guides = guideData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during AJAX guide search for term: '{SearchTerm}'", searchTerm);
                return Json(new { success = false, message = "Search failed. Please try again." });
            }
        }

        /// <summary>
        /// Display create guide form
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View(new AdminGuideCreateViewModel());
        }

        /// <summary>
        /// Handle guide creation (form POST)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminGuideCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                var guide = new GuideModel
                {
                    FirstName = vm.FirstName,
                    LastName = vm.LastName,
                    Bio = vm.Bio,
                    Email = vm.Email,
                    PhoneNumber = vm.PhoneNumber,
                    YearsExperience = vm.YearsExperience,
                    PhotoUrl = null
                };

                var result = await _guideService.CreateGuideAsync(guide);
                if (result != null)
                {
                    _logger.LogInformation("Successfully created guide: {Name}", guide.FullName);
                    TempData["SuccessMessage"] = "Guide created successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    _logger.LogWarning("Failed to create guide: {Name}", guide.FullName);
                    ModelState.AddModelError("", "Failed to create guide. Please try again.");
                    return View(vm);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating guide: {Name}", vm.FullName);
                ModelState.AddModelError("", "An error occurred while creating the guide. Please try again.");
                return View(vm);
            }
        }

        /// <summary>
        /// AJAX handler for validating guide form
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Validate([FromBody] AdminGuideCreateViewModel guide)
        {
            ModelState.Clear();
            TryValidateModel(guide);

            var errors = new Dictionary<string, List<string>>();

            foreach (var modelError in ModelState)
            {
                if (modelError.Value.Errors.Count > 0)
                {
                    errors[modelError.Key] = modelError.Value.Errors.Select(e => e.ErrorMessage).ToList();
                }
            }

            if (!string.IsNullOrEmpty(guide.Email))
            {
                try
                {
                    var existingGuides = await _guideService.GetAllGuidesAsync();
                    if (existingGuides.Any(g => g.Email?.Equals(guide.Email, StringComparison.OrdinalIgnoreCase) == true))
                    {
                        if (!errors.ContainsKey("Email"))
                            errors["Email"] = new List<string>();
                        errors["Email"].Add("Email address is already in use by another guide.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error checking email uniqueness during validation");
                }
            }

            return Json(new { isValid = errors.Count == 0, errors });
        }

        /// <summary>
        /// AJAX handler for creating guide
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] AdminGuideCreateViewModel guide)
        {
            try
            {
                ModelState.Clear();
                TryValidateModel(guide);

                if (!ModelState.IsValid)
                {
                    var errors = new Dictionary<string, List<string>>();
                    foreach (var modelError in ModelState)
                    {
                        if (modelError.Value.Errors.Count > 0)
                        {
                            errors[modelError.Key] = modelError.Value.Errors.Select(e => e.ErrorMessage).ToList();
                        }
                    }
                    return Json(new { success = false, errors });
                }

                if (!string.IsNullOrEmpty(guide.Email))
                {
                    var existingGuides = await _guideService.GetAllGuidesAsync();
                    if (existingGuides.Any(g => g.Email?.Equals(guide.Email, StringComparison.OrdinalIgnoreCase) == true))
                    {
                        return Json(new
                        {
                            success = false,
                            errors = new Dictionary<string, List<string>>
                            {
                                ["Email"] = new List<string> { "Email address is already in use by another guide." }
                            }
                        });
                    }
                }

                var guideModel = new GuideModel
                {
                    FirstName = guide.FirstName,
                    LastName = guide.LastName,
                    Bio = guide.Bio,
                    Email = guide.Email,
                    PhoneNumber = guide.PhoneNumber,
                    YearsExperience = guide.YearsExperience,
                    PhotoUrl = null
                };

                var result = await _guideService.CreateGuideAsync(guideModel);
                if (result != null)
                {
                    _logger.LogInformation("Successfully created guide via AJAX: {Name}", guideModel.FullName);
                    return Json(new
                    {
                        success = true,
                        message = "Guide created successfully!",
                        guide = new
                        {
                            id = result.Id,
                            fullName = result.FullName,
                            email = result.Email
                        }
                    });
                }
                else
                {
                    _logger.LogWarning("Failed to create guide via AJAX: {Name}", guideModel.FullName);
                    return Json(new
                    {
                        success = false,
                        message = "Failed to create guide. Please try again."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating guide via AJAX: {Name}", guide.FullName);
                return Json(new
                {
                    success = false,
                    message = "An error occurred while creating the guide. Please try again."
                });
            }
        }

        /// <summary>
        /// Display edit guide form
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var guide = await _guideService.GetGuideByIdAsync(id);
            if (guide == null)
            {
                return NotFound();
            }

            var vm = new AdminGuideEditViewModel
            {
                Id = guide.Id,
                FirstName = guide.FirstName,
                LastName = guide.LastName,
                Bio = guide.Bio,
                Email = guide.Email ?? string.Empty,
                PhoneNumber = guide.PhoneNumber,
                YearsExperience = guide.YearsExperience
            };

            return View(vm);
        }

        /// <summary>
        /// Handle guide update (form POST)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminGuideEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                var guide = new GuideModel
                {
                    Id = vm.Id,
                    FirstName = vm.FirstName,
                    LastName = vm.LastName,
                    Bio = vm.Bio,
                    Email = vm.Email,
                    PhoneNumber = vm.PhoneNumber,
                    YearsExperience = vm.YearsExperience,
                    PhotoUrl = null
                };

                var result = await _guideService.UpdateGuideAsync(vm.Id, guide);
                if (result != null)
                {
                    _logger.LogInformation("Successfully updated guide: {Name}", guide.FullName);
                    TempData["SuccessMessage"] = "Guide updated successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    _logger.LogWarning("Failed to update guide: {Name}", guide.FullName);
                    ModelState.AddModelError("", "Failed to update guide. Please try again.");
                    return View(vm);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating guide: {Name}", vm.FullName);
                ModelState.AddModelError("", "An error occurred while updating the guide. Please try again.");
                return View(vm);
            }
        }

        /// <summary>
        /// AJAX handler for validating edit form
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ValidateEdit([FromBody] AdminGuideEditViewModel guide)
        {
            ModelState.Clear();
            TryValidateModel(guide);

            var errors = new Dictionary<string, List<string>>();

            foreach (var modelError in ModelState)
            {
                if (modelError.Value.Errors.Count > 0)
                {
                    errors[modelError.Key] = modelError.Value.Errors.Select(e => e.ErrorMessage).ToList();
                }
            }

            if (!string.IsNullOrEmpty(guide.Email))
            {
                try
                {
                    var existingGuides = await _guideService.GetAllGuidesAsync();
                    if (existingGuides.Any(g => g.Id != guide.Id &&
                                               g.Email?.Equals(guide.Email, StringComparison.OrdinalIgnoreCase) == true))
                    {
                        if (!errors.ContainsKey("Email"))
                            errors["Email"] = new List<string>();
                        errors["Email"].Add("Email address is already in use by another guide.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error checking email uniqueness during validation");
                }
            }

            return Json(new { isValid = errors.Count == 0, errors });
        }

        /// <summary>
        /// AJAX handler for updating guide
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateAjax([FromBody] AdminGuideEditViewModel guide)
        {
            try
            {
                ModelState.Clear();
                TryValidateModel(guide);

                if (!ModelState.IsValid)
                {
                    var errors = new Dictionary<string, List<string>>();
                    foreach (var modelError in ModelState)
                    {
                        if (modelError.Value.Errors.Count > 0)
                        {
                            errors[modelError.Key] = modelError.Value.Errors.Select(e => e.ErrorMessage).ToList();
                        }
                    }
                    return Json(new { success = false, errors });
                }

                if (!string.IsNullOrEmpty(guide.Email))
                {
                    var existingGuides = await _guideService.GetAllGuidesAsync();
                    if (existingGuides.Any(g => g.Id != guide.Id &&
                                               g.Email?.Equals(guide.Email, StringComparison.OrdinalIgnoreCase) == true))
                    {
                        return Json(new
                        {
                            success = false,
                            errors = new Dictionary<string, List<string>>
                            {
                                ["Email"] = new List<string> { "Email address is already in use by another guide." }
                            }
                        });
                    }
                }

                var guideModel = new GuideModel
                {
                    Id = guide.Id,
                    FirstName = guide.FirstName,
                    LastName = guide.LastName,
                    Bio = guide.Bio,
                    Email = guide.Email,
                    PhoneNumber = guide.PhoneNumber,
                    YearsExperience = guide.YearsExperience,
                    PhotoUrl = null
                };

                var result = await _guideService.UpdateGuideAsync(guide.Id, guideModel);
                if (result != null)
                {
                    _logger.LogInformation("Successfully updated guide via AJAX: {Name}", guideModel.FullName);
                    return Json(new
                    {
                        success = true,
                        message = "Guide updated successfully!",
                        guide = new
                        {
                            id = result.Id,
                            fullName = result.FullName,
                            email = result.Email
                        }
                    });
                }
                else
                {
                    _logger.LogWarning("Failed to update guide via AJAX: {Name}", guideModel.FullName);
                    return Json(new
                    {
                        success = false,
                        message = "Failed to update guide. Please try again."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating guide via AJAX: {FullName}", guide.FullName);
                return Json(new
                {
                    success = false,
                    message = "An error occurred while updating the guide. Please try again."
                });
            }
        }

        /// <summary>
        /// Display guide details
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var vm = new AdminGuideDetailsViewModel();

            try
            {
                _logger.LogInformation("Loading guide details for ID: {GuideId}", id);

                var guide = await _guideService.GetGuideByIdAsync(id);

                if (guide == null)
                {
                    _logger.LogWarning("Guide not found: {GuideId}", id);
                    return NotFound();
                }

                vm.Guide = guide;
                vm.TripsCount = 0;

                _logger.LogInformation("Successfully loaded guide details: {GuideName}", guide.FullName);
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving guide details: {GuideId}", id);
                vm.ErrorMessage = "An error occurred while retrieving the guide details.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Handle guide deletion via AJAX POST
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Admin attempting to delete guide with ID: {GuideId}", id);

                var guide = await _guideService.GetGuideByIdAsync(id);
                if (guide == null)
                {
                    _logger.LogWarning("Attempted to delete non-existent guide: {GuideId}", id);
                    return Json(new { success = false, message = "Guide not found." });
                }

                var success = await _guideService.DeleteGuideAsync(id);

                if (success)
                {
                    _logger.LogInformation("Successfully deleted guide: {GuideName} (ID: {GuideId})", guide.FullName, id);
                    TempData["SuccessMessage"] = $"Guide '{guide.FullName}' has been deleted successfully.";

                    return Json(new { success = true, message = $"Guide '{guide.FullName}' deleted successfully." });
                }
                else
                {
                    _logger.LogWarning("Failed to delete guide: {GuideName} (ID: {GuideId})", guide.FullName, id);
                    return Json(new { success = false, message = "Failed to delete guide. The guide may be assigned to active trips." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while deleting guide with ID: {GuideId}", id);
                return Json(new { success = false, message = "An error occurred while deleting the guide." });
            }
        }
    }
}

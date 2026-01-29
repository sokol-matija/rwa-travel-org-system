using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDestinationService _destinationService;

        public HomeController(ILogger<HomeController> logger, IDestinationService destinationService)
        {
            _logger = logger;
            _destinationService = destinationService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var vm = new HomeViewModel();

            try
            {
                _logger.LogInformation("Loading featured destinations for home page");
                var allDestinations = await _destinationService.GetAllDestinationsAsync();
                vm.FeaturedDestinations = allDestinations.Take(3).ToList();
                _logger.LogInformation("Loaded {Count} featured destinations", vm.FeaturedDestinations.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading featured destinations");
                vm.ErrorMessage = "Unable to load featured destinations at this time.";
            }

            return View(vm);
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var vm = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            return View(vm);
        }
    }
}

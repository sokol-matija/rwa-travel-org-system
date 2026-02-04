using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Services;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminLogsController : Controller
    {
        private readonly ILogService _logService;
        private readonly ILogger<AdminLogsController> _logger;

        private const int DefaultPageSize = 50;
        private const int MaxPageSize = 100;

        public AdminLogsController(
            ILogService logService,
            ILogger<AdminLogsController> logger)
        {
            _logService = logService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = DefaultPageSize)
        {
            var vm = new AdminLogsIndexViewModel
            {
                Page = page < 1 ? 1 : page,
                PageSize = pageSize < 1 || pageSize > MaxPageSize ? DefaultPageSize : pageSize
            };

            try
            {
                _logger.LogInformation("Loading logs page - Page: {Page}, PageSize: {PageSize}", vm.Page, vm.PageSize);

                vm.IsLoading = true;

                var (logs, totalCount) = await _logService.GetLogsAsync(vm.Page, vm.PageSize);

                vm.Logs = logs;
                vm.TotalCount = totalCount;

                _logger.LogInformation("Loaded {LogCount} logs out of {TotalCount} for page {Page}",
                    vm.Logs.Count, vm.TotalCount, vm.Page);

                vm.IsLoading = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading logs page");
                vm.ErrorMessage = "Unable to load system logs. Please try again later.";
                vm.IsLoading = false;

                vm.Logs = new List<LogModel>();
                vm.TotalCount = 0;
            }

            return View(vm);
        }
    }
}

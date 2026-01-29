using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Services;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthService authService, ILogger<AccountController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_authService.IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl ?? Url.Content("~/")
            };
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.LoginAsync(model);

            if (result)
            {
                _logger.LogInformation("User {Username} logged in successfully", model.Username);

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return LocalRedirect(model.ReturnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your username and password.");
            _logger.LogWarning("Failed login attempt for user {Username}", model.Username);
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            if (_authService.IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.RegisterAsync(model);

            if (result)
            {
                _logger.LogInformation("User {Username} registered successfully", model.Username);
                TempData["SuccessMessage"] = "Registration successful! You can now log in with your credentials.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError(string.Empty, "Registration failed. Username or email may already be in use.");
            _logger.LogWarning("Failed registration attempt for user {Username}", model.Username);
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var vm = new ProfileViewModel();

            try
            {
                _logger.LogInformation("User is authenticated: {IsAuthenticated}", User.Identity?.IsAuthenticated);

                var token = HttpContext.Session.GetString("Token");
                _logger.LogInformation("Token exists in session: {HasToken}", !string.IsNullOrEmpty(token));

                if (string.IsNullOrEmpty(token))
                {
                    var authResult = await HttpContext.AuthenticateAsync();
                    if (authResult.Succeeded)
                    {
                        token = authResult.Properties?.GetTokenValue("access_token");
                    }
                }

                vm.CurrentUser = await _authService.GetCurrentUserAsync();

                if (vm.CurrentUser == null)
                {
                    _logger.LogWarning("Failed to load user profile: User not found");
                    vm.ErrorMessage = "Failed to load your profile information. Please try again later.";
                    vm.DetailedError = "API call returned null. Check logs for more details.";
                }
                else
                {
                    _logger.LogInformation("Successfully loaded profile for user: {Username}", vm.CurrentUser.Username);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user profile");
                vm.ErrorMessage = "An error occurred while loading your profile. Please try again later.";
                vm.DetailedError = $"Exception: {ex.Message}";
            }

            return View(vm);
        }

        public async Task<IActionResult> Logout()
        {
            if (_authService.IsAuthenticated())
            {
                await _authService.LogoutAsync();
                _logger.LogInformation("User logged out");
            }

            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _authService.ChangePasswordAsync(
                    model.CurrentPassword,
                    model.NewPassword,
                    model.ConfirmPassword);

                if (result)
                {
                    _logger.LogInformation("User changed their password successfully.");
                    TempData["SuccessMessage"] = "Your password has been changed successfully.";
                    return RedirectToAction("ChangePassword");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Password change failed. Please check your current password and try again.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                TempData["ErrorMessage"] = "An error occurred while changing your password. Please try again later.";
                return View(model);
            }
        }
    }
}

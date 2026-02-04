using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(LoginViewModel loginModel);

        Task<bool> RegisterAsync(RegisterViewModel registerModel);

        Task LogoutAsync();

        Task<UserModel?> GetCurrentUserAsync();

        bool IsAuthenticated();

        bool IsAdmin();

        Task<bool> ChangePasswordAsync(string currentPassword, string newPassword, string confirmPassword);
    }
}

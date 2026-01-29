using WebApp.Models;

namespace WebApp.ViewModels
{
    public class ProfileViewModel
    {
        public UserModel? CurrentUser { get; set; }
        public string? ErrorMessage { get; set; }
        public string? DetailedError { get; set; }
    }
}

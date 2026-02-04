using WebApp.Models;

namespace WebApp.Services
{
    public interface IGuideService
    {
        Task<IEnumerable<GuideModel>> GetAllGuidesAsync();

        Task<GuideModel?> GetGuideByIdAsync(int id);

        Task<GuideModel?> CreateGuideAsync(GuideModel guide);

        Task<GuideModel?> UpdateGuideAsync(int id, GuideModel guide);

        Task<bool> DeleteGuideAsync(int id);

        Task<IEnumerable<GuideModel>> GetGuidesByTripAsync(int tripId);
    }
}

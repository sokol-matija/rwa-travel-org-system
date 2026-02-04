using WebApp.Models;

namespace WebApp.Services
{
    public interface ITripRegistrationService
    {
        Task<List<TripRegistrationModel>> GetAllRegistrationsAsync();

        Task<TripRegistrationModel?> GetRegistrationByIdAsync(int id);

        Task<List<TripRegistrationModel>> GetRegistrationsByUserAsync(int userId);

        Task<List<TripRegistrationModel>> GetRegistrationsByTripAsync(int tripId);

        Task<TripRegistrationModel?> CreateRegistrationAsync(TripRegistrationModel registration);

        Task<TripRegistrationModel?> UpdateRegistrationAsync(int id, TripRegistrationModel registration);

        Task<bool> DeleteRegistrationAsync(int id);

        Task<bool> UpdateRegistrationStatusAsync(int id, string status);
    }
}

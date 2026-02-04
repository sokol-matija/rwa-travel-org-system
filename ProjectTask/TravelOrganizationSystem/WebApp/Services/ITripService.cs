using WebApp.Models;

namespace WebApp.Services
{
    public interface ITripService
    {
        Task<List<TripModel>> GetAllTripsAsync();

        Task<(List<TripModel> trips, int totalCount)> GetTripsAsync(int page = 1, int pageSize = 10, int? destinationId = null);

        Task<(List<TripModel> trips, int totalCount)> SearchTripsAsync(string? name, string? description, int page = 1, int pageSize = 10);

        Task<TripModel?> GetTripByIdAsync(int id);

        Task<List<TripModel>> GetTripsByDestinationAsync(int destinationId);

        Task<TripModel?> CreateTripAsync(TripModel trip);

        Task<TripModel?> UpdateTripAsync(int id, TripModel trip);

        Task<bool> DeleteTripAsync(int id);

        Task<bool> AssignGuideToTripAsync(int tripId, int guideId);

        Task<bool> RemoveGuideFromTripAsync(int tripId, int guideId);

        Task<bool> BookTripAsync(int tripId, int numberOfParticipants);

        Task<List<TripRegistrationModel>> GetUserTripsAsync();

        Task<bool> CancelBookingAsync(int bookingId);
    }
}

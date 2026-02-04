using WebApp.Models;

namespace WebApp.Services
{
    public interface IDestinationService
    {
        Task<List<DestinationModel>> GetAllDestinationsAsync();

        Task<DestinationModel?> GetDestinationByIdAsync(int id);

        Task<DestinationModel?> CreateDestinationAsync(DestinationModel destination);

        Task<DestinationModel?> UpdateDestinationAsync(int id, DestinationModel destination);

        Task<bool> DeleteDestinationAsync(int id);
    }
}

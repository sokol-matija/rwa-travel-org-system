using WebApp.Models;

namespace WebApp.Services
{
    public interface ILogService
    {
        Task<List<LogModel>> GetLogsAsync(int count);

        Task<int> GetLogsCountAsync();

        Task<(List<LogModel> logs, int totalCount)> GetLogsAsync(int page = 1, int pageSize = 50);
    }
}

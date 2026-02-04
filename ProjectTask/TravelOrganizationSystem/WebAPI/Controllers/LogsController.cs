using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class LogsController : ControllerBase
    {
        private readonly ILogService _logService;

        public LogsController(ILogService logService)
        {
            _logService = logService;
        }

        [HttpGet("get/{count}")]
        public async Task<IActionResult> Get(int count)
        {
            if (count <= 0)
                return BadRequest("Count must be greater than 0");

            var logs = await _logService.GetLogsAsync(count);
            return Ok(logs);
        }

        [HttpGet("count")]
        public async Task<IActionResult> Count()
        {
            var count = await _logService.GetLogsCountAsync();
            return Ok(new { count });
        }
    }
}

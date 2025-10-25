using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketsService.Database;

namespace TicketsService.Controllers
{
    [ApiController]
    [Route("manage")]
    public class HealthController : ControllerBase
    {
        private readonly TicketsContext _dbContext;

        public HealthController(TicketsContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                var canConnect = await _dbContext.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    return StatusCode(503, new
                    {
                        status = "Unhealthy",
                        timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        checks = new
                        {
                            database = new { status = "Unhealthy", error = "Cannot connect to database" }
                        }
                    });
                }

                var ticketsCount = await _dbContext.Tickets.CountAsync();
                
                var result = new
                {
                    status = "Healthy",
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    checks = new
                    {
                        database = new { status = "Healthy", ticketsCount = ticketsCount }
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    status = "Unhealthy",
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    error = ex.Message
                });
            }
        }
    }
}
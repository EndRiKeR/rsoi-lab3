using Microsoft.AspNetCore.Mvc;

namespace GatewayService.Controllers
{
    [ApiController]
    [Route("manage")]
    public class HealthController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HealthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                var services = new[]
                {
                    new { Name = "FlightService", Url = "http://flight-service:8060/manage/health" },
                    new { Name = "TicketsService", Url = "http://tickets-service:8070/manage/health" },
                    new { Name = "BonusService", Url = "http://bonus-service:8050/manage/health" }
                };

                var healthResults = new List<object>();
                var overallHealthy = true;

                foreach (var service in services)
                {
                    try
                    {
                        var client = _httpClientFactory.CreateClient();
                        var response = await client.GetAsync(service.Url);
                        var isHealthy = response.IsSuccessStatusCode;
                        
                        healthResults.Add(new
                        {
                            service = service.Name,
                            status = isHealthy ? "Healthy" : "Unhealthy",
                            responseTime = response.Headers.Date?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                        });

                        if (!isHealthy)
                            overallHealthy = false;
                    }
                    catch (Exception ex)
                    {
                        healthResults.Add(new
                        {
                            service = service.Name,
                            status = "Unhealthy",
                            error = ex.Message
                        });
                        overallHealthy = false;
                    }
                }

                var result = new
                {
                    status = overallHealthy ? "Healthy" : "Degraded",
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    checks = healthResults
                };

                return overallHealthy ? Ok(result) : StatusCode(503, result);
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
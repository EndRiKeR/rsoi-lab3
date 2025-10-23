using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Common.DtoModels.BonusServiceDto;
using Common.DtoModels.ErrorDto;
using Common.DtoModels.FlightServiceDto;
using Common.DtoModels.GatewayDto;
using Common.DtoModels.TicketsServiceDto;

namespace GatewayService.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class GatewayController : ControllerBase
    {
        private readonly HttpClient _ticketsClient;
        private readonly HttpClient _flightsClient;
        private readonly HttpClient _privilegeClient;
        
        public GatewayController(IHttpClientFactory httpClientFactory)
        {
            _privilegeClient = httpClientFactory.CreateClient("BonusService");
            _flightsClient = httpClientFactory.CreateClient("FlightService");
            _ticketsClient = httpClientFactory.CreateClient("TicketsService");
        }
        
        [HttpGet("flights")]
        public async Task<IActionResult> GetFlights([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            try
            {
                var response = await _flightsClient.GetAsync($"/api/v1/flights?page={page}&size={size}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var flights = JsonSerializer.Deserialize<PaginationResponse>(content);
                    return Ok(flights);
                }
                
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
        
        [HttpGet("flights/{flightNumber}")]
        public async Task<IActionResult> GetFlights([FromRoute] string flightNumber)
        {
            try
            {
                var response = await _flightsClient.GetAsync($"/api/v1/flights/{flightNumber}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var flight = JsonSerializer.Deserialize<FlightResponse>(content);
                    return Ok(flight);
                }
                
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
        
        [HttpGet("tickets")]
        public async Task<IActionResult> GetUserTickets()
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var usernameValue))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                string? username = usernameValue[0];
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/tickets");
                request.Headers.Add("X-User-Name", username);
                
                var response = await _ticketsClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var tickets = JsonSerializer.Deserialize<List<TicketResponse>>(content);
                    return Ok(tickets);
                }
                
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
        
        [HttpGet("tickets/{ticketUid}")]
        public async Task<IActionResult> GetTicket(Guid ticketUid)
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var usernameValue))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                string? username = usernameValue[0];
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/tickets/{ticketUid}");
                request.Headers.Add("X-User-Name", username);
                
                var response = await _ticketsClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var ticket = JsonSerializer.Deserialize<TicketResponse>(content);
                    return Ok(ticket);
                }
                
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
        
        [HttpPost("tickets")]
        public async Task<IActionResult> BuyTicket([FromBody] TicketPurchaseRequest requestDto)
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var usernameValue))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                string? username = usernameValue[0];
                var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tickets")
                {
                    Content = JsonContent.Create(requestDto)
                };
                request.Headers.Add("X-User-Name", username);
                
                var response = await _ticketsClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var purchaseResponse = JsonSerializer.Deserialize<TicketPurchaseResponse>(content);
                    return Ok(purchaseResponse);
                }
                
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
        
        [HttpDelete("tickets/{ticketUid}")]
        public async Task<IActionResult> ReturnTicket(Guid ticketUid)
        {
            try
            {
                Console.WriteLine("-*-*-*-*-*-*-*-*-*-start");
                if (!Request.Headers.TryGetValue("X-User-Name", out var usernameValue))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                string? username = usernameValue[0];
                var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/tickets/{ticketUid}");
                request.Headers.Add("X-User-Name", username);
                
                var response = await _ticketsClient.SendAsync(request);
                Console.WriteLine($"-*-*-*-*-*-*-*-*-*- {request}");
                Console.WriteLine($"-*-*-*-*-*-*-*-*-*- {response}");
                
                if (response.IsSuccessStatusCode)
                {
                    return NoContent();
                }
                
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
        
        [HttpGet("me")]
        public async Task<IActionResult> GetUserInfo()
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var usernameValue))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                string? username = usernameValue[0];
                var ticketsRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/tickets");
                ticketsRequest.Headers.Add("X-User-Name", username);
                var ticketsResponse = await _ticketsClient.SendAsync(ticketsRequest);
                
                var bonusRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/privilege");
                bonusRequest.Headers.Add("X-User-Name", username);
                var bonusResponse = await _privilegeClient.SendAsync(bonusRequest);
                
                if (ticketsResponse.IsSuccessStatusCode && bonusResponse.IsSuccessStatusCode)
                {
                    var ticketsContent = await ticketsResponse.Content.ReadAsStringAsync();
                    var bonusContent = await bonusResponse.Content.ReadAsStringAsync();
                    
                    var tickets = JsonSerializer.Deserialize<List<TicketResponse>>(ticketsContent);
                    var privilege = JsonSerializer.Deserialize<PrivilegeShortInfo>(bonusContent);
                    
                    var userInfo = new UserInfoResponse
                    {
                        Tickets = tickets ?? new List<TicketResponse>(),
                        Privilege = privilege ?? new PrivilegeShortInfo()
                    };
                    
                    return Ok(userInfo);
                }
                
                return StatusCode(500, new ErrorResponse { Message = "Error getting user info" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
        
        [HttpGet("privilege")]
        public async Task<IActionResult> GetPrivilegeInfo()
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var usernameValue))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                string? username = usernameValue[0];
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/privilege");
                request.Headers.Add("X-User-Name", username);
                
                var response = await _privilegeClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var privilegeInfo = JsonSerializer.Deserialize<PrivilegeInfoResponse>(content);
                    return Ok(privilegeInfo);
                }
                
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
        
        [HttpPost("privilege/update-balance")]
        public async Task<IActionResult> UpdatePrivilegeInfo([FromBody] UpdateBalanceHistoryRequest historyRequest)
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var usernameValue))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                string? username = usernameValue[0];

                var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/privilege/update-balance")
                {
                    Content = JsonContent.Create(historyRequest)
                };
                request.Headers.Add("X-User-Name", username);
                var response = await _privilegeClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var privilegeInfo = JsonSerializer.Deserialize<PrivilegeInfoResponse>(content);
                    return Ok(privilegeInfo);
                }
                
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
    }
}
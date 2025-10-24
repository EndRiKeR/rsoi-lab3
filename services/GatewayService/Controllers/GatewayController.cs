using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Common.CircuitBreaker;
using Common.CircuitBreaker.Enums;
using Common.DtoModels.BonusServiceDto;
using Common.DtoModels.ErrorDto;
using Common.DtoModels.FlightServiceDto;
using Common.DtoModels.GatewayDto;
using Common.DtoModels.TicketsServiceDto;
using Common.Fallbacks;

namespace GatewayService.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class GatewayController : ControllerBase
    {
        private readonly HttpClient _ticketsClient;
        private readonly HttpClient _flightsClient;
        private readonly HttpClient _privilegeClient;
        private readonly CircuitBreakersController _circuitBreakersController;
        private readonly ControllersFallbacks _fallbacks;
        
        public GatewayController(
            IHttpClientFactory httpClientFactory,
            CircuitBreakersController circuitBreakersController,
            ControllersFallbacks fallbacks)
        {
            _privilegeClient = httpClientFactory.CreateClient("BonusService");
            _flightsClient = httpClientFactory.CreateClient("FlightService");
            _ticketsClient = httpClientFactory.CreateClient("TicketsService");
            
            _circuitBreakersController = circuitBreakersController;
            _fallbacks = fallbacks;
        }
        
        [HttpGet("flights")]
        public async Task<IActionResult> GetFlights([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/flights");
                
                PaginationResponse? response = await _circuitBreakersController.ExecuteAsync(
                    Services.Flights,
                    async () => await SendRequest<PaginationResponse>(_flightsClient, request),
                    () => _fallbacks.GetFlightsFallback()
                );
                
                return Ok(response);
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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/flights/{flightNumber}");
                
                FlightResponse? response = await _circuitBreakersController.ExecuteAsync(
                    Services.Flights,
                    async () => await SendRequest<FlightResponse>(_flightsClient, request),
                    () => _fallbacks.GetFlightDataFallback(flightNumber)
                );
                
                return Ok(response);
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
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/tickets");
                request.Headers.Add("X-User-Name", usernameValue[0]);
                
                TicketResponse? response = await _circuitBreakersController.ExecuteAsync(
                    Services.Tickets,
                    async () => await SendRequest<TicketResponse>(_ticketsClient, request),
                    () => _fallbacks.GetTicketsFallback()
                );
                
                return Ok(response);
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
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/tickets/{ticketUid}");
                request.Headers.Add("X-User-Name", usernameValue[0]);
                
                TicketResponse? response = await _circuitBreakersController.ExecuteAsync(
                    Services.Tickets,
                    async () => await SendRequest<TicketResponse>(_ticketsClient, request),
                    () => _fallbacks.GetTicketsFallback(ticketUid)
                );
                
                return Ok(response);
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
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tickets")
                {
                    Content = JsonContent.Create(requestDto)
                };
                request.Headers.Add("X-User-Name", usernameValue[0]);

                var response = await _ticketsClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                
                var content = await response.Content.ReadAsStringAsync();
                var purchaseResponse = JsonSerializer.Deserialize<TicketPurchaseResponse>(content);
                return Ok(purchaseResponse);
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
                if (!Request.Headers.TryGetValue("X-User-Name", out var usernameValue))
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                
                string? username = usernameValue[0];
                var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/tickets/{ticketUid}");
                request.Headers.Add("X-User-Name", username);
                
                var response = await _ticketsClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                    return NoContent();
                
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
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                
                string? username = usernameValue[0];
                
                var ticketsRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/tickets");
                ticketsRequest.Headers.Add("X-User-Name", username);
                
                List<TicketResponse>? ticketsResponse = await _circuitBreakersController.ExecuteAsync(
                    Services.Tickets,
                    async () => await SendRequest<List<TicketResponse>>(_ticketsClient, ticketsRequest),
                    () => _fallbacks.GetAllTicketsFallback()
                );
                
                var bonusRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/privilege");
                bonusRequest.Headers.Add("X-User-Name", username);
                
                PrivilegeShortInfo? bonusResponse = await _circuitBreakersController.ExecuteAsync(
                    Services.Bonuses,
                    async () => await SendRequest<PrivilegeShortInfo>(_privilegeClient, bonusRequest),
                    () => _fallbacks.GetPrivilegeShortInfoFallback()
                );
                    
                var userInfo = new UserInfoResponse
                {
                    Tickets = ticketsResponse ?? new List<TicketResponse>(),
                    Privilege = bonusResponse ?? new PrivilegeShortInfo()
                };
                    
                return Ok(userInfo);

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
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                
                string? username = usernameValue[0];
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/privilege");
                request.Headers.Add("X-User-Name", username);
                
                PrivilegeInfoResponse? response = await _circuitBreakersController.ExecuteAsync(
                    Services.Flights,
                    async () => await SendRequest<PrivilegeInfoResponse>(_privilegeClient, request),
                    () => _fallbacks.GetPrivilegeInfoResponseFallback()
                );
                
                return Ok(response);
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
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                
                string? username = usernameValue[0];

                var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/privilege/update-balance")
                {
                    Content = JsonContent.Create(historyRequest)
                };
                request.Headers.Add("X-User-Name", username);
                var response = await _privilegeClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                
                var content = await response.Content.ReadAsStringAsync();
                var privilegeInfo = JsonSerializer.Deserialize<PrivilegeInfoResponse>(content);
                return Ok(privilegeInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
        
        private async Task<T?> SendRequest<T>(HttpClient client, HttpRequestMessage requestMessage)
        {
            HttpResponseMessage response = await client.SendAsync(requestMessage);
                
            // ошибку обработает щиток и я не достану текст, так что нет особой разницы, что кидать
            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());
                
            string content = await response.Content.ReadAsStringAsync();
            T? responseModel = JsonSerializer.Deserialize<T>(content);
            
            return responseModel;
        }
        
        
        
    }
}
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Common.CircuitBreaker;
using Common.CircuitBreaker.Enums;
using Common.DtoModels.BonusServiceDto;
using Common.DtoModels.ErrorDto;
using Common.DtoModels.FlightServiceDto;
using Common.DtoModels.GatewayDto;
using Common.DtoModels.TicketsServiceDto;
using Common.Errors;
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
                    Services.Flight,
                    async () => await SendRequest<PaginationResponse>(_flightsClient, request)
                );
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                if (ex is ServerDiedException)
                    return StatusCode(503, new ErrorResponse { Message = ex.Message });

                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
        
        [HttpGet("flights/{flightNumber}")]
        public async Task<IActionResult> GetFlights([FromRoute] string flightNumber)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/flights/{flightNumber}");
                
                FlightResponse? response = await _circuitBreakersController.ExecuteSoftAsync(
                    Services.Flight,
                    async () => await SendRequest<FlightResponse>(_flightsClient, request),
                    () => _fallbacks.GetFlightDataFallback(flightNumber)
                );
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                if (ex is ServerDiedException)
                    return StatusCode(503, new ErrorResponse { Message = ex.Message });
                
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
                
                List<TicketResponse>? response = await _circuitBreakersController.ExecuteSoftAsync(
                    Services.Ticket,
                    async () => await SendRequest<List<TicketResponse>>(_ticketsClient, request),
                    () => _fallbacks.GetAllTicketsFallback()
                );
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                if (ex is ServerDiedException)
                    return StatusCode(503, new ErrorResponse { Message = ex.Message });
                
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
                
                TicketResponse? response = await _circuitBreakersController.ExecuteSoftAsync(
                    Services.Ticket,
                    async () => await SendRequest<TicketResponse>(_ticketsClient, request),
                    () => _fallbacks.GetTicketsFallback(ticketUid)
                );
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                if (ex is ServerDiedException)
                    return StatusCode(503, new ErrorResponse { Message = ex.Message });
                
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
        
        [HttpPost("tickets")]
        public async Task<IActionResult> BuyTicket([FromBody] TicketPurchaseRequest requestDto)
        {
            bool isTicketAdded = false;
            Guid ticketUid = Guid.NewGuid();
            
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var usernameValue))
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                
                // запрос на полет
                // если не найден - ошибка
                var flightRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/flights/{requestDto.FlightNumber}");
                FlightResponse? flightResponse = await _circuitBreakersController.ExecuteAsync(
                    Services.Flight,
                    async () => await SendRequest<FlightResponse>(_flightsClient, flightRequest)
                );
                flightRequest.Headers.Add("X-User-Name", usernameValue[0]);
                
                // пытаемся создать запись о новом билете
                // нет - ошибка
                var addTicketRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/tickets/")
                {
                    Content = JsonContent.Create(new TicketAddRequest
                    {
                        TicketUid = ticketUid,
                        FlightNumber = requestDto.FlightNumber,
                        Username = usernameValue[0],
                        Price = requestDto.Price,
                        Status = "PAID"
                    })
                };
                addTicketRequest.Headers.Add("X-User-Name", usernameValue[0]);
                
                TicketAddResponse? ticketResponse = await _circuitBreakersController.ExecuteAsync(
                    Services.Ticket,
                    async () => await SendRequest<TicketAddResponse>(_ticketsClient, addTicketRequest)
                );

                isTicketAdded = true;
                
                // если оплачиваем бонусами - запросик к бонусам
                // нет - ошибка + удаляем добавленный билет
                var (paidByBonuses, paidByMoney) = await CalculatePayment(requestDto.Price, requestDto.PaidFromBalance, usernameValue);
                var privilegeInfo = await UpdateBonusBalance(usernameValue[0], ticketUid, paidByBonuses, requestDto.Price);
                
                var response = new TicketPurchaseResponse
                {
                    TicketUid = ticketUid,
                    FlightNumber = requestDto.FlightNumber,
                    FromAirport = flightResponse.FromAirport,
                    ToAirport = flightResponse.ToAirport, 
                    Date = DateTime.Now,
                    Price = requestDto.Price,
                    PaidByMoney = paidByMoney,
                    PaidByBonuses = paidByBonuses,
                    Status = "PAID",
                    Privilege = privilegeInfo
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                if (isTicketAdded)
                {
                    var addTicketRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/tickets/{ticketUid}");
                    await _ticketsClient.SendAsync(addTicketRequest);
                }
                
                if (ex is ServerDiedException)
                    return StatusCode(503, new ErrorResponse { Message = ex.Message });
                
                return StatusCode(503, new ErrorResponse { Message = ex.Message });
            }
        }
        
        [HttpDelete("tickets/{ticketUid}")]
        public async Task<IActionResult> ReturnTicket(Guid ticketUid)
        {
            try
            {
                // обновить статус билета
                // нет - ошибка
                if (!Request.Headers.TryGetValue("X-User-Name", out var usernameValue))
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                
                string? username = usernameValue[0];
                var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/tickets/{ticketUid}");
                request.Headers.Add("X-User-Name", username);
                
                var response = await _ticketsClient.SendAsync(request);
                
                // запрос к бонусам для отката траты/получения
                // нет - все ок + бесконечный ретрай запроса
                if (!response.IsSuccessStatusCode)
                {
                    var bonusRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/privilege/return-balance");
                    bonusRequest.Headers.Add("X-User-Name", username);
                    var bonusResponse = await _privilegeClient.SendAsync(bonusRequest);

                    if (!bonusResponse.IsSuccessStatusCode)
                    {
                        // to repeat try
                    }
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                if (ex is ServerDiedException)
                    return StatusCode(503, new ErrorResponse { Message = ex.Message });
                
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
                
                List<TicketResponse>? ticketsResponse = await _circuitBreakersController.ExecuteSoftAsync(
                    Services.Ticket,
                    async () => await SendRequest<List<TicketResponse>>(_ticketsClient, ticketsRequest),
                    () => _fallbacks.GetAllTicketsFallback()
                );
                
                var bonusRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/privilege");
                bonusRequest.Headers.Add("X-User-Name", username);
                
                PrivilegeShortInfo? bonusResponse = await _circuitBreakersController.ExecuteSoftAsync(
                    Services.Bonus,
                    async () => await SendRequest<PrivilegeShortInfo>(_privilegeClient, bonusRequest),
                    () => _fallbacks.GetPrivilegeShortInfoFallback()
                );
                    
                var userInfo = new UserInfoResponse
                {
                    Tickets = ticketsResponse ?? new List<TicketResponse>(),
                    Privilege = bonusResponse ?? new PrivilegeShortInfo(),
                };
                    
                if (bonusResponse?.Status == "ERROR")
                {
                    var safeTicketsResponse = ticketsResponse ?? new List<TicketResponse>();
                    return Ok(safeTicketsResponse);
                }
        
                return Ok(userInfo);

            }
            catch (Exception ex)
            {
                if (ex is ServerDiedException)
                    return StatusCode(503, new ErrorResponse { Message = ex.Message });
                
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
                    Services.Bonus,
                    async () => await SendRequest<PrivilegeInfoResponse>(_privilegeClient, request)
                );
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                if (ex is ServerDiedException)
                    return StatusCode(503, new ErrorResponse { Message = ex.Message });
                
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
                if (ex is ServerDiedException)
                    return StatusCode(503, new ErrorResponse { Message = ex.Message });
                
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
        
        private async Task<(int paidByBonuses, int paidByMoney)> CalculatePayment(int ticketPrice, bool paidFromBalance, string username)
        {
            if (!paidFromBalance)
                return (0, ticketPrice);
            
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/privilege");
            request.Headers.Add("X-User-Name", username);
            
            PrivilegeInfoResponse? response = await _circuitBreakersController.ExecuteSoftAsync(
                Services.Flight,
                async () => await SendRequest<PrivilegeInfoResponse>(_privilegeClient, request),
                () => _fallbacks.GetPrivilegeInfoResponseFallback()
            );

            if (response != null)
            {
                var availableBonuses = response.Balance;
                var bonusesToUse = Math.Min(availableBonuses, ticketPrice);
                
                return (bonusesToUse, ticketPrice - bonusesToUse);
            }
            
            return (0, ticketPrice);
        }
        
        private async Task<PrivilegeShortInfo> UpdateBonusBalance(string username, Guid ticketUid, int paidByBonuses, int ticketPrice)
        {
            bool isPaidByBonuses = paidByBonuses != 0;

            var bonusAmount = (int)(ticketPrice * 0.1);
            
            var debitRequest = new UpdateBalanceHistoryRequest
            {
                TicketUid = ticketUid,
                BalanceDiff = isPaidByBonuses ? -paidByBonuses : bonusAmount,
                OperationType = isPaidByBonuses ? "DEBIT_THE_ACCOUNT" : "FILL_IN_BALANCE"
            };
            
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/privilege/update-balance")
            {
                Content = JsonContent.Create(debitRequest)
            };
            httpRequest.Headers.Add("X-User-Name", username);
            
            await _privilegeClient.SendAsync(httpRequest);
            
            var privilegeRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/privilege");
            privilegeRequest.Headers.Add("X-User-Name", username);
            
            PrivilegeInfoResponse? response = await _circuitBreakersController.ExecuteAsync(
                Services.Bonus,
                async () => await SendRequest<PrivilegeInfoResponse>(_privilegeClient, privilegeRequest)
            );

            return new PrivilegeShortInfo
            {
                Balance = response?.Balance ?? 0,
                Status = response?.Status ?? "BRONZE"
            };
        }
    }
}
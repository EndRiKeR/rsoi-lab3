using Common.DtoModels.BonusServiceDto;
using Common.DtoModels.ErrorDto;
using Common.DtoModels.TicketsServiceDto;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Common.DtoModels.FlightServiceDto;
using TicketsService.Database.Enums;
using TicketsService.Database.Models;
using TicketsService.Database.Repositories.Interfaces;

namespace TicketsService.Controllers
{
    [ApiController]
    [Route("api/v1/tickets")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly HttpClient _gatewayClient;
        
        public TicketsController(ITicketRepository ticketRepository, IHttpClientFactory httpClientFactory)
        {
            _ticketRepository = ticketRepository;
            _gatewayClient = httpClientFactory.CreateClient("Gateway");
        }
        
        [HttpGet]
        public async Task<IActionResult> GetUserTickets()
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var usernameValues))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                var username = usernameValues[0];
                var allTickets = await _ticketRepository.GetAll();
                var userTickets = allTickets.Where(t => t.Username == username).ToList();

                List<TicketResponse> ticketResponses = new();
                foreach (var userTicket in userTickets)
                {
                    var flightData = await GetFlightByNumber(userTicket.FlightNumber);
                    ticketResponses.Add(new TicketResponse
                    {
                        TicketUid = userTicket.TicketUid,
                        FlightNumber = userTicket.FlightNumber,
                        FromAirport = flightData.FromAirport,
                        ToAirport = flightData.ToAirport,
                        Date = DateTime.Now,
                        Price = userTicket.Price,
                        Status = userTicket.Status.ToString()
                    });
                }
                
                return Ok(ticketResponses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
        
        [HttpGet("{ticketUid}")]
        public async Task<IActionResult> GetTicket(Guid ticketUid)
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var username))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                var allTickets = await _ticketRepository.GetAll();
                var ticket = allTickets.FirstOrDefault(t => t.TicketUid == ticketUid && t.Username == username.ToString());
                
                if (ticket == null)
                    return NotFound(new ErrorResponse { Message = "Ticket not found" });
                
                var flightData = await GetFlightByNumber(ticket.FlightNumber);
                
                var response = new TicketResponse
                {
                    TicketUid = ticket.TicketUid,
                    FlightNumber = ticket.FlightNumber,
                    FromAirport = flightData.FromAirport,
                    ToAirport = flightData.ToAirport,
                    Date = DateTime.Now,
                    Price = ticket.Price,
                    Status = ticket.Status.ToString()
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> BuyTicket([FromBody] TicketPurchaseRequest request)
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var username))
                {
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                }
                
                var usernameValue = username.ToString();
                var ticketUid = Guid.NewGuid();
                
                var (paidByBonuses, paidByMoney) = await CalculatePayment(request.Price, request.PaidFromBalance, usernameValue);
                
                var ticket = new Ticket
                {
                    TicketUid = ticketUid,
                    Username = usernameValue,
                    FlightNumber = request.FlightNumber,
                    Price = request.Price,
                    Status = TicketStatusConverter.ToStatus("PAID")
                };
                
                var createdTicket = await _ticketRepository.Add(ticket);
                
                var privilegeInfo = await UpdateBonusBalance(usernameValue, ticketUid, paidByBonuses, paidByMoney, request.Price);
                
                var flightData = await GetFlightByNumber(request.FlightNumber);
                
                var response = new TicketPurchaseResponse
                {
                    TicketUid = createdTicket.TicketUid,
                    FlightNumber = createdTicket.FlightNumber,
                    FromAirport = flightData.FromAirport,
                    ToAirport = flightData.ToAirport, 
                    Date = DateTime.Now,
                    Price = createdTicket.Price,
                    PaidByMoney = paidByMoney,
                    PaidByBonuses = paidByBonuses,
                    Status = createdTicket.Status.ToString(),
                    Privilege = privilegeInfo
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
        
        [HttpDelete("{ticketUid}")]
        public async Task<IActionResult> ReturnTicket(Guid ticketUid)
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-User-Name", out var username))
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                
                var usernameValue = username.ToString();
                var allTickets = await _ticketRepository.GetAll();
                var ticket = allTickets.FirstOrDefault(t => t.TicketUid == ticketUid && t.Username == usernameValue);
                
                if (ticket == null)
                    return NotFound(new ErrorResponse { Message = "Ticket not found" });
                
                ticket.Status = TicketStatusConverter.ToStatus("CANCELED");
                await _ticketRepository.Update(ticket);
                await ReturnBonusBalance(usernameValue, ticketUid);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
        
        private async Task<(int paidByBonuses, int paidByMoney)> CalculatePayment(int ticketPrice, bool paidFromBalance, string username)
        {
            if (!paidFromBalance)
            {
                return (0, ticketPrice);
            }
            
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/privilege");
            request.Headers.Add("X-User-Name", username);
            
            var response = await _gatewayClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var privilegeInfo = JsonSerializer.Deserialize<PrivilegeInfoResponse>(content);
                
                if (privilegeInfo != null)
                {
                    var availableBonuses = privilegeInfo.Balance;
                    var bonusesToUse = Math.Min(availableBonuses, ticketPrice);
                    
                    return (bonusesToUse, ticketPrice - bonusesToUse);
                }
            }
            
            return (0, ticketPrice);
        }
        
        private async Task<PrivilegeShortInfo> UpdateBonusBalance(string username, Guid ticketUid, int paidByBonuses, int paidByMoney, int ticketPrice)
        {
            bool isPaidByBonuses = paidByBonuses > 0;

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
            
            await _gatewayClient.SendAsync(httpRequest);
            
            var privilegeRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/privilege");
            privilegeRequest.Headers.Add("X-User-Name", username);
            
            var privilegeResponse = await _gatewayClient.SendAsync(privilegeRequest);
            if (privilegeResponse.IsSuccessStatusCode)
            {
                var content = await privilegeResponse.Content.ReadAsStringAsync();
                var privilegeInfo = JsonSerializer.Deserialize<PrivilegeInfoResponse>(content);
                
                return new PrivilegeShortInfo
                {
                    Balance = privilegeInfo?.Balance ?? 0,
                    Status = privilegeInfo?.Status ?? "BRONZE"
                };
            }
            
            return new PrivilegeShortInfo();
        }
        
        private async Task ReturnBonusBalance(string username, Guid ticketUid)
        {
            var historyRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/privilege");
            historyRequest.Headers.Add("X-User-Name", username);
            
            var historyResponse = await _gatewayClient.SendAsync(historyRequest);
            if (historyResponse.IsSuccessStatusCode)
            {
                var content = await historyResponse.Content.ReadAsStringAsync();
                var privilegeInfo = JsonSerializer.Deserialize<PrivilegeInfoResponse>(content);
                
                if (privilegeInfo != null)
                {
                    var ticketHistory = privilegeInfo.History
                        .FirstOrDefault(h => h.TicketUid == ticketUid);
                    
                    if (ticketHistory != null)
                    {
                        var returnOperation = new UpdateBalanceHistoryRequest
                        {
                            TicketUid = ticketUid,
                            BalanceDiff = -ticketHistory.BalanceDiff,
                            OperationType = ticketHistory.OperationType == "DEBIT_THE_ACCOUNT" 
                                ? "FILL_IN_BALANCE" 
                                : "DEBIT_THE_ACCOUNT"
                        };
                        
                        var returnRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/privilege/update-balance")
                        {
                            Content = JsonContent.Create(returnOperation)
                        };
                        returnRequest.Headers.Add("X-User-Name", username);
                        
                        await _gatewayClient.SendAsync(returnRequest);
                    }
                }
            }
        }
        
        private async Task<FlightResponse> GetFlightByNumber(string flightNumber)
        {
            var flightRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/flights/{flightNumber}");
            var flightResponse = await _gatewayClient.SendAsync(flightRequest);

            if (!flightResponse.IsSuccessStatusCode)
            {
                Console.WriteLine(flightResponse.StatusCode);
                return null;
            }
            
            var content = await flightResponse.Content.ReadAsStringAsync();
            var privilegeInfo = JsonSerializer.Deserialize<FlightResponse>(content);
            
            Console.WriteLine(content);
                
            return privilegeInfo;
        }
    }
}
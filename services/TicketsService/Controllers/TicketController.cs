using Common.DtoModels.BonusServiceDto;
using Common.DtoModels.ErrorDto;
using Common.DtoModels.TicketsServiceDto;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Common.DtoModels.FlightServiceDto;
using Common.Errors;
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
                    return BadRequest(new ErrorResponse { Message = "X-User-Name header is required" });
                
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
        public async Task<IActionResult> AddTicket([FromBody] TicketAddRequest request)
        {
            try
            {
                var ticket = new Ticket
                {
                    TicketUid = request.TicketUid,
                    Username = request.Username,
                    FlightNumber = request.FlightNumber,
                    Price = request.Price,
                    Status = TicketStatusConverter.ToStatus(request.Status)
                };

                await _ticketRepository.Add(ticket);
                
                return Ok(new TicketAddResponse { TicketUid = ticket.TicketUid });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
        
        [HttpDelete]
        public async Task<IActionResult> DeleteTicket([FromBody] TicketDeleteRequest request)
        {
            try
            {
                await _ticketRepository.DeleteByTicketUid(request.TicketUid);
                return NoContent();
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
                
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
        
        private async Task<FlightResponse> GetFlightByNumber(string flightNumber)
        {
            var flightRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/flights/{flightNumber}");
            var flightResponse = await _gatewayClient.SendAsync(flightRequest);

            if (!flightResponse.IsSuccessStatusCode)
                return null;
            
            var content = await flightResponse.Content.ReadAsStringAsync();
            var flight= JsonSerializer.Deserialize<FlightResponse>(content);
            
            Console.WriteLine(content);
                
            return flight;
        }
    }
}
using Common.DtoModels.ErrorDto;
using Common.DtoModels.FlightServiceDto;
using FlightService.Database.Models;
using FlightService.Database.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlightService.Controllers
{
    [ApiController]
    [Route("api/v1/flights")]
    public class FlightsController : ControllerBase
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IAirportRepository _airportRepository;
        
        public FlightsController(IFlightRepository flightRepository, IAirportRepository airportRepository)
        {
            _flightRepository = flightRepository;
            _airportRepository = airportRepository;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetFlights([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            try
            {
                var allFlights = await _flightRepository.GetAll();
                
                var totalElements = allFlights.Count;
                var pagedFlights = allFlights
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToList();
                
                var flightResponses = new List<FlightResponse>();
                foreach (var flight in pagedFlights)
                {
                    string fromAirportName = "Unknown";
                    string toAirportName = "Unknown";
                    
                    if (flight.FromAirportId.HasValue)
                    {
                        var fromAirport = await _airportRepository.GetById(flight.FromAirportId.Value);
                        fromAirportName = $"{fromAirport.City} {fromAirport.Name}";
                    }
                    
                    if (flight.ToAirportId.HasValue)
                    {
                        var toAirport = await _airportRepository.GetById(flight.ToAirportId.Value);
                        toAirportName = $"{toAirport.City} {toAirport.Name}";
                    }
                    
                    flightResponses.Add(new FlightResponse
                    {
                        FlightNumber = flight.FlightNumber,
                        FromAirport = fromAirportName,
                        ToAirport = toAirportName,
                        Date = flight.DateTime,
                        Price = flight.Price
                    });
                }
                
                var response = new PaginationResponse
                {
                    Page = page,
                    PageSize = size,
                    TotalElements = totalElements,
                    Items = flightResponses
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
        
        [HttpGet("{flightNumber}")]
        public async Task<IActionResult> GetFlightByNumber([FromRoute] string flightNumber)
        {
            try
            {
                var flights = await _flightRepository.GetFlightsByFlightNumber(flightNumber);
                var flight = flights.FirstOrDefault();
                
                if (flight == null)
                {
                    return NotFound(new ErrorResponse { Message = $"Flight with number {flightNumber} not found" });
                }
                
                string fromAirportName = "Unknown";
                string toAirportName = "Unknown";
                
                if (flight.FromAirportId.HasValue)
                {
                    var fromAirport = await _airportRepository.GetById(flight.FromAirportId.Value);
                    fromAirportName = $"{fromAirport.City} {fromAirport.Name}";
                }
                
                if (flight.ToAirportId.HasValue)
                {
                    var toAirport = await _airportRepository.GetById(flight.ToAirportId.Value);
                    toAirportName = $"{toAirport.City} {toAirport.Name}";
                }
                
                var response = new FlightResponse
                {
                    FlightNumber = flight.FlightNumber,
                    FromAirport = fromAirportName,
                    ToAirport = toAirportName,
                    Date = flight.DateTime,
                    Price = flight.Price
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
        
        [HttpPost("fillDB")]
        public async Task<IActionResult> FillDatabase()
        {
            try
            {
                var airports = new List<Airport>
                {
                    new Airport { Name = "Шереметьево", City = "Москва", Country = "Россия" },
                    new Airport { Name = "Пулково", City = "Санкт-Петербург", Country = "Россия" }
                };
                
                await _airportRepository.AddList(airports);
                
                var flights = new List<Flight>
                {
                    new Flight
                    {
                        FlightNumber = "AFL031",
                        DateTime = DateTime.Parse("2021-10-08 20:00"),
                        FromAirportId = airports[1].Id,
                        ToAirportId = airports[0].Id,
                        Price = 1500
                    }
                };
                
                await _flightRepository.AddList(flights);
                
                return Ok("Test data created successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse { Message = ex.Message });
            }
        }
    }
}
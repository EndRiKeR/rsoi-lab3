using FlightService.Database.Models;
using FlightService.Database.Repositories.Interfaces;

namespace FlightService.Database;

public class DatabaseFiller
{
    private readonly IFlightRepository _flightRepository;
    private readonly IAirportRepository _airportRepository;
    
    public DatabaseFiller(
        IFlightRepository flightRepository,
        IAirportRepository airportRepository)
    {
        _flightRepository = flightRepository;
        _airportRepository = airportRepository;
    }

    public async Task AddTestData()
    {
        try
        {
            if ((await _airportRepository.GetAll()).Count == 0)
                await AddAirport();
            
            if ((await _flightRepository.GetAll()).Count == 0)
                await AddFlight();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    private async Task AddAirport()
    {
        var airports = new List<Airport>
        {
            new Airport()
            {
                Id = 1,
                Name = "Шереметьево",
                City = "Москва",
                Country = "Россия"
            },
            new Airport()
            {
                Id = 2,
                Name = "Пулково",
                City = "Санкт-Петербург",
                Country = "Россия"
            },
        };

        await _airportRepository.AddList(airports);
    }

    private async Task AddFlight()
    {
        var dateTime = new DateTime(2021, 10, 8, 20, 0, 0);
        
        var flight = new Flight()
        {
            Id = 1,
            FlightNumber = "AFL031",
            DateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            Price = 1500,
            FromAirportId = 2,
            ToAirportId = 1
        };
        
        await _flightRepository.Add(flight);
    }
}
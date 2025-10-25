using Common.Interfaces;
using FlightService.Database.Models;

namespace FlightService.Database.Repositories.Interfaces;

public interface IFlightRepository : IRepository<Flight>
{
    Task<List<Flight>> GetFlightsByAirport(long airportId);
    Task<List<Flight>> GetFlightsByFlightNumber(string flightNumber);
    Task<List<Flight>> SearchFlights(long? fromAirportId, long? toAirportId, DateTime? date);
}
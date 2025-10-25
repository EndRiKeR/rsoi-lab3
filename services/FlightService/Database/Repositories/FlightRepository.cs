using FlightService.Database.Models;
using FlightService.Database.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightService.Database.Repositories;

public class FlightRepository : IFlightRepository
{
    private readonly FlightContext _context;
    
    public FlightRepository(FlightContext context)
    {
        _context = context;
    }
    
    public async Task<List<Flight>> GetAll()
    {
        try
        {
            return await _context.Flights
                .Include(f => f.FromAirport)
                .Include(f => f.ToAirport)
                .ToListAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Flight> GetById(long id)
    {
        try
        {
            var flight = await _context.Flights
                .Include(f => f.FromAirport)
                .Include(f => f.ToAirport)
                .FirstOrDefaultAsync(f => f.Id == id);
            
            if (flight == null)
                throw new Exception($"Flight with id {id} not found");
            
            return flight;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Flight> Add(Flight flight)
    {
        try
        {
            var exists = await _context.Flights
                .AnyAsync(f => f.Id == flight.Id);
            
            if (exists)
                throw new Exception("Flight already exists");
            
            var newFlight = await _context.Flights.AddAsync(flight);
            await _context.SaveChangesAsync();
            
            return newFlight.Entity;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<Flight>> AddList(List<Flight> addFlights)
    {
        try
        {
            var existingIds = await _context.Flights
                .Where(f => addFlights.Select(x => x.Id).Contains(f.Id))
                .Select(f => f.Id)
                .ToListAsync();

            if (existingIds.Any())
                throw new Exception($"Flights with ids {string.Join(", ", existingIds)} already exist");

            await _context.Flights.AddRangeAsync(addFlights);
            await _context.SaveChangesAsync();
            
            return addFlights;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task Delete(long id)
    {
        try
        {
            var flight = await _context.Flights.FirstOrDefaultAsync(f => f.Id == id);
            
            if (flight == null)
                throw new Exception($"Flight with id {id} not found");
            
            _context.Flights.Remove(flight);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Flight> Update(Flight flight)
    {
        try
        {
            var existingFlight = await _context.Flights.FirstOrDefaultAsync(f => f.Id == flight.Id);
            
            if (existingFlight == null)
                throw new Exception($"Flight with id {flight.Id} not found");

            // Обновляем свойства
            existingFlight.FlightNumber = flight.FlightNumber;
            existingFlight.DateTime = flight.DateTime;
            existingFlight.Price = flight.Price;
            existingFlight.FromAirportId = flight.FromAirportId;
            existingFlight.ToAirportId = flight.ToAirportId;

            await _context.SaveChangesAsync();
            
            return existingFlight;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    // Дополнительные методы для работы с рейсами
    public async Task<List<Flight>> GetFlightsByAirport(long airportId)
    {
        try
        {
            return await _context.Flights
                .Include(f => f.FromAirport)
                .Include(f => f.ToAirport)
                .Where(f => f.FromAirportId == airportId || f.ToAirportId == airportId)
                .ToListAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<Flight>> GetFlightsByFlightNumber(string flightNumber)
    {
        try
        {
            return await _context.Flights
                .Include(f => f.FromAirport)
                .Include(f => f.ToAirport)
                .Where(f => f.FlightNumber == flightNumber)
                .ToListAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<Flight>> SearchFlights(long? fromAirportId, long? toAirportId, DateTime? date)
    {
        try
        {
            var query = _context.Flights
                .Include(f => f.FromAirport)
                .Include(f => f.ToAirport)
                .AsQueryable();

            if (fromAirportId.HasValue)
                query = query.Where(f => f.FromAirportId == fromAirportId);

            if (toAirportId.HasValue)
                query = query.Where(f => f.ToAirportId == toAirportId);

            if (date.HasValue)
            {
                var startDate = date.Value.Date;
                var endDate = startDate.AddDays(1);
                query = query.Where(f => f.DateTime >= startDate && f.DateTime < endDate);
            }

            return await query.ToListAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
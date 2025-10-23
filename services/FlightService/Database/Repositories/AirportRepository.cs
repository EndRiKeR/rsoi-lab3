using FlightService.Database.Models;
using FlightService.Database.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightService.Database.Repositories;

public class AirportRepository : IAirportRepository
{
    private readonly FlightContext _context;
    
    public AirportRepository(FlightContext context)
    {
        _context = context;
    }
    
    public async Task<List<Airport>> GetAll()
    {
        try
        {
            return await _context.Airports.ToListAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Airport> GetById(long id)
    {
        try
        {
            var airport = await _context.Airports.FirstOrDefaultAsync(a => a.Id == id);
            
            if (airport == null)
                throw new Exception($"Airport with id {id} not found");
            
            return airport;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Airport> Add(Airport airport)
    {
        try
        {
            var exists = await _context.Airports.AnyAsync(a => a.Id == airport.Id);
            
            if (exists)
                throw new Exception("Airport already exists");
            
            var newAirport = await _context.Airports.AddAsync(airport);
            
            await _context.SaveChangesAsync();
            return newAirport.Entity;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<Airport>> AddList(List<Airport> addAirports)
    {
        try
        {
            var existingIds = await _context.Airports
                .Where(a => addAirports.Select(x => x.Id).Contains(a.Id))
                .Select(a => a.Id)
                .ToListAsync();

            if (existingIds.Any())
                throw new Exception($"Airports with ids {string.Join(", ", existingIds)} already exist");

            await _context.Airports.AddRangeAsync(addAirports);
            await _context.SaveChangesAsync();
            
            return addAirports;
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
            var airport = await _context.Airports.FirstOrDefaultAsync(a => a.Id == id);
            
            if (airport == null)
                throw new Exception($"Airport with id {id} not found");
            
            _context.Airports.Remove(airport);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Airport> Update(Airport airport)
    {
        try
        {
            var existingAirport = await _context.Airports.FirstOrDefaultAsync(a => a.Id == airport.Id);
            
            if (existingAirport == null)
                throw new Exception($"Airport with id {airport.Id} not found");

            existingAirport.Name = airport.Name;
            existingAirport.City = airport.City;
            existingAirport.Country = airport.Country;

            await _context.SaveChangesAsync();
            return existingAirport;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
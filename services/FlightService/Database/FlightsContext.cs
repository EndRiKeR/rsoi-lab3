using FlightService.Database.ContextConfigurations;
using FlightService.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FlightService.Database;

public class FlightContext : DbContext
{
    public virtual DbSet<Airport> Airports { get; set; }
    public virtual DbSet<Flight> Flights { get; set; }
    
    public FlightContext() { }
    public FlightContext(DbContextOptions<FlightContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new FlightsConfiguration());
        modelBuilder.ApplyConfiguration(new AirportsConfiguration());
    }
}
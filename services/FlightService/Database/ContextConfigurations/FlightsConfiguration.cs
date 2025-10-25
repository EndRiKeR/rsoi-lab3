using FlightService.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightService.Database.ContextConfigurations;

public class FlightsConfiguration : IEntityTypeConfiguration<Flight>
{
    public void Configure(EntityTypeBuilder<Flight> builder)
    {
        builder.HasKey(e => e.Id);
            
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.FlightNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.DateTime)
            .IsRequired();

        builder.Property(e => e.Price)
            .IsRequired();

        builder.HasOne(f => f.FromAirport)
            .WithMany(a => a.DepartureFlights)
            .HasForeignKey(f => f.FromAirportId)
            .OnDelete(DeleteBehavior.Restrict); 

        builder.HasOne(f => f.ToAirport)
            .WithMany(a => a.ArrivalFlights)
            .HasForeignKey(f => f.ToAirportId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.FlightNumber);
        builder.HasIndex(e => e.DateTime);
        builder.HasIndex(e => e.FromAirportId);
        builder.HasIndex(e => e.ToAirportId);
    }
}

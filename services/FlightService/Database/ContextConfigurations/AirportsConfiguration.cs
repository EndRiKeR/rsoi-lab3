using FlightService.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightService.Database.ContextConfigurations;

public class AirportsConfiguration : IEntityTypeConfiguration<Airport>
{
    public void Configure(EntityTypeBuilder<Airport> builder)
    {
        builder.HasKey(e => e.Id);
            
        builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();

        builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

        builder.Property(e => e.City)
                .IsRequired()
                .HasMaxLength(255);

        builder.Property(e => e.Country)
                .IsRequired()
                .HasMaxLength(255);

        builder.HasIndex(e => new { e.City, e.Name });
    }
}

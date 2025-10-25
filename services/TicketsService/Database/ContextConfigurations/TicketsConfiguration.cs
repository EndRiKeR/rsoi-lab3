using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketsService.Database.Models;

namespace TicketsService.Database.ContextConfigurations;

public class TicketsConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.TicketUid)
            .IsUnique();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.Username)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(e => e.FlightNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.Price)
            .IsRequired();
    }
}

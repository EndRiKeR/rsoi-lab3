using Microsoft.EntityFrameworkCore;
using TicketsService.Database.ContextConfigurations;
using TicketsService.Database.Models;

namespace TicketsService.Database;

public class TicketsContext : DbContext
{
    public virtual DbSet<Ticket> Tickets { get; set; }

    public TicketsContext() { }

    public TicketsContext(DbContextOptions<TicketsContext> dbContextOptions) : base(dbContextOptions) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new TicketsConfiguration());
    }
}

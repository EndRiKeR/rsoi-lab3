using BonusService.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BonusService.Database.ContextConfigurations;

public class PrivilegeConfiguration : IEntityTypeConfiguration<Privilege>
{
    public void Configure(EntityTypeBuilder<Privilege> builder)
    {
        builder.HasKey(e => e.Id);
            
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Username)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(80)
            .HasConversion<string>();

        builder.Property(e => e.Balance)
            .IsRequired(false); // NULLABLE

        builder.HasIndex(e => e.Username)
            .IsUnique();
    }
}

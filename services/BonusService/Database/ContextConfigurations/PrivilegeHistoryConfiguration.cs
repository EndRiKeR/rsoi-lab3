using BonusService.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BonusService.Database.ContextConfigurations;

public class PrivilegeHistoryConfiguration : IEntityTypeConfiguration<PrivilegeHistory>
{
    public void Configure(EntityTypeBuilder<PrivilegeHistory> builder)
    {
        builder.HasKey(e => e.Id);
            
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.TicketUid)
            .IsRequired();

        builder.Property(e => e.Datetime)
            .IsRequired();

        builder.Property(e => e.BalanceDiff)
            .IsRequired();

        builder.Property(e => e.OperationType)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        // Внешний ключ
        builder.HasOne(ph => ph.Privilege)
            .WithMany(p => p.History)
            .HasForeignKey(ph => ph.PrivilegeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Индексы
        builder.HasIndex(e => e.PrivilegeId);
        builder.HasIndex(e => e.TicketUid);
        builder.HasIndex(e => e.Datetime);
    }
}

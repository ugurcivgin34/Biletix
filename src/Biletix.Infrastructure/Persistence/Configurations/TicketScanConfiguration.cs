using Biletix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biletix.Infrastructure.Persistence.Configurations;

/// <summary>
/// TicketScan entity'si icin tablo, kolon ve indeks ayarlarini tanimlar.
/// </summary>
public sealed class TicketScanConfiguration : IEntityTypeConfiguration<TicketScan>
{
    /// <summary>
    /// TicketScan entity'sinin EF Core model konfigurasyonunu uygular.
    /// </summary>
    /// <param name="builder">TicketScan entity builder'i.</param>
    public void Configure(EntityTypeBuilder<TicketScan> builder)
    {
        builder.ToTable("TicketScans");

        builder.HasKey(scan => scan.Id);

        builder.Property(scan => scan.ScannedBy)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(scan => scan.InvalidReason)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.HasIndex(scan => scan.BookingId);
        builder.HasIndex(scan => scan.EventId);
        builder.HasIndex(scan => scan.ScannedAt);
    }
}

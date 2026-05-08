using Biletix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biletix.Infrastructure.Persistence.Configurations;

/// <summary>
/// TicketType entity'si icin tablo, fiyat kolonu ve indeks ayarlarini tanimlar.
/// </summary>
public class TicketTypeConfiguration : IEntityTypeConfiguration<TicketType>
{
    /// <summary>
    /// TicketType entity'sinin EF Core model konfigurasyonunu uygular.
    /// </summary>
    /// <param name="builder">TicketType entity builder'i.</param>
    public void Configure(EntityTypeBuilder<TicketType> builder)
    {
        builder.ToTable("TicketTypes");

        builder.HasKey(ticketType => ticketType.Id);

        builder.Property(ticketType => ticketType.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ticketType => ticketType.Price)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(ticketType => ticketType.EventId);
    }
}

using Biletix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biletix.Infrastructure.Persistence.Configurations;

/// <summary>
/// BookingItem entity'si icin tablo, fiyat kolonu ve TicketType iliskisini tanimlar.
/// </summary>
public class BookingItemConfiguration : IEntityTypeConfiguration<BookingItem>
{
    /// <summary>
    /// BookingItem entity'sinin EF Core model konfigurasyonunu uygular.
    /// </summary>
    /// <param name="builder">BookingItem entity builder'i.</param>
    public void Configure(EntityTypeBuilder<BookingItem> builder)
    {
        builder.ToTable("BookingItems");

        builder.HasKey(bookingItem => bookingItem.Id);

        builder.Property(bookingItem => bookingItem.UnitPrice)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(bookingItem => bookingItem.TicketType)
            .WithMany()
            .HasForeignKey(bookingItem => bookingItem.TicketTypeId);
    }
}

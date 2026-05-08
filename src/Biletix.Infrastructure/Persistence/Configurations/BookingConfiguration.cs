using Biletix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biletix.Infrastructure.Persistence.Configurations;

/// <summary>
/// Booking aggregate'i icin tablo, iliski, enum conversion ve indeks ayarlarini tanimlar.
/// </summary>
public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    /// <summary>
    /// Booking entity'sinin EF Core model konfigurasyonunu uygular.
    /// </summary>
    /// <param name="builder">Booking entity builder'i.</param>
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(booking => booking.Id);

        builder.Property(booking => booking.Status)
            .HasConversion<string>();

        builder.Property(booking => booking.TotalAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(booking => booking.IdempotencyKey)
            .HasMaxLength(100);

        builder.Property(booking => booking.PaymentIntentId)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.HasIndex(booking => booking.IdempotencyKey)
            .IsUnique();

        builder.HasMany(booking => booking.Items)
            .WithOne()
            .HasForeignKey(bookingItem => bookingItem.BookingId);

        builder.Navigation(booking => booking.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne(booking => booking.Event)
            .WithMany()
            .HasForeignKey(booking => booking.EventId);

        builder.HasIndex(booking => booking.UserId);
        builder.HasIndex(booking => booking.Status);
        builder.HasIndex(booking => booking.ExpiresAt);
    }
}

using Biletix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biletix.Infrastructure.Persistence.Configurations;

/// <summary>
/// Venue aggregate'i icin tablo, kolon ve constraint ayarlarini tanimlar.
/// </summary>
public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    /// <summary>
    /// Venue entity'sinin EF Core model konfigurasyonunu uygular.
    /// </summary>
    /// <param name="builder">Venue entity builder'i.</param>
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("Venues");

        builder.HasKey(venue => venue.Id);

        builder.Property(venue => venue.Id)
            .HasColumnType("uuid");

        builder.Property(venue => venue.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(venue => venue.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(venue => venue.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(venue => venue.SeatMapJson)
            .HasColumnType("jsonb");
    }
}

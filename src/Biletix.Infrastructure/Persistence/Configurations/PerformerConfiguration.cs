using Biletix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biletix.Infrastructure.Persistence.Configurations;

/// <summary>
/// Performer aggregate'i icin tablo ve kolon ayarlarini tanimlar.
/// </summary>
public class PerformerConfiguration : IEntityTypeConfiguration<Performer>
{
    /// <summary>
    /// Performer entity'sinin EF Core model konfigurasyonunu uygular.
    /// </summary>
    /// <param name="builder">Performer entity builder'i.</param>
    public void Configure(EntityTypeBuilder<Performer> builder)
    {
        builder.ToTable("Performers");

        builder.HasKey(performer => performer.Id);

        builder.Property(performer => performer.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(performer => performer.Genre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(performer => performer.ImageUrl)
            .HasMaxLength(500)
            .IsRequired(false);
    }
}

using Biletix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biletix.Infrastructure.Persistence.Configurations;

/// <summary>
/// OutboxMessage entity'si icin tablo, kolon ve indeks ayarlarini tanimlar.
/// </summary>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    /// <summary>
    /// OutboxMessage entity'sinin EF Core model konfigurasyonunu uygular.
    /// </summary>
    /// <param name="builder">OutboxMessage entity builder'i.</param>
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.EventType)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(message => message.Payload)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(message => message.Error)
            .HasColumnType("text");

        builder.HasIndex(message => new { message.IsProcessed, message.CreatedAt });
    }
}

using Biletix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biletix.Infrastructure.Persistence.Configurations;

/// <summary>
/// Event aggregate'i icin tablo, iliski, enum conversion ve indeks ayarlarini tanimlar.
/// </summary>
public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    /// <summary>
    /// Event entity'sinin EF Core model konfigurasyonunu uygular.
    /// </summary>
    /// <param name="builder">Event entity builder'i.</param>
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(@event => @event.Id);

        builder.Property(@event => @event.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(@event => @event.Description)
            .HasMaxLength(2000);

        builder.Property(@event => @event.Status)
            .HasConversion<string>();

        builder.Property(@event => @event.CreatedBy)
            .IsRequired();

        builder.Property(@event => @event.ImageUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.HasOne(@event => @event.Venue)
            .WithMany()
            .HasForeignKey(@event => @event.VenueId);

        builder.HasOne(@event => @event.Performer)
            .WithMany()
            .HasForeignKey(@event => @event.PerformerId);

        builder.HasMany(@event => @event.TicketTypes)
            .WithOne()
            .HasForeignKey(ticketType => ticketType.EventId);

        builder.Navigation(@event => @event.TicketTypes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(@event => @event.StartDate);
        builder.HasIndex(@event => @event.Status);
        builder.HasIndex(@event => @event.VenueId);
        builder.HasIndex(@event => @event.CreatedBy);
    }
}

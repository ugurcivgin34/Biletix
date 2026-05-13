using System.Linq.Expressions;
using Biletix.Application.Common.Interfaces;
using Biletix.Domain.Base;
using Biletix.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Infrastructure.Persistence;

/// <summary>
/// Uygulamanin EF Core veritabani baglamidir; audit alanlarini set eder ve domain event'leri yayinlar.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Veritabani baglami icin EF Core ayarlarini ve domain event yayini icin mediator'u alir.
    /// </summary>
    /// <param name="options">DbContext konfigurasyonu.</param>
    /// <param name="mediator">Domain event'leri yayinlamak icin kullanilan mediator.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMediator mediator)
        : base(options)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Kullanici aggregate'leri icin sorgu ve kalicilik giris noktasi.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Mekan aggregate'leri icin sorgu ve kalicilik giris noktasi.
    /// </summary>
    public DbSet<Venue> Venues => Set<Venue>();

    /// <summary>
    /// Performer aggregate'leri icin sorgu ve kalicilik giris noktasi.
    /// </summary>
    public DbSet<Performer> Performers => Set<Performer>();

    /// <summary>
    /// Etkinlik aggregate'leri icin sorgu ve kalicilik giris noktasi.
    /// </summary>
    public DbSet<Event> Events => Set<Event>();

    /// <summary>
    /// Etkinliklere ait bilet tipleri icin sorgu ve kalicilik giris noktasi.
    /// </summary>
    public DbSet<TicketType> TicketTypes => Set<TicketType>();

    /// <summary>
    /// Rezervasyon aggregate'leri icin sorgu ve kalicilik giris noktasi.
    /// </summary>
    public DbSet<Booking> Bookings => Set<Booking>();

    /// <summary>
    /// Rezervasyon kalemleri icin sorgu ve kalicilik giris noktasi.
    /// </summary>
    public DbSet<BookingItem> BookingItems => Set<BookingItem>();

    /// <summary>
    /// Transactional outbox mesajlari icin sorgu ve kalicilik giris noktasi.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>
    /// QR bilet tarama kayitlari icin sorgu ve kalicilik giris noktasi.
    /// </summary>
    public DbSet<TicketScan> TicketScans => Set<TicketScan>();

    /// <summary>
    /// Degisiklikleri kaydetmeden once audit alanlarini gunceller, kayittan sonra domain event'leri yayinlar.
    /// </summary>
    /// <param name="cancellationToken">Asenkron islemi iptal etmek icin kullanilan token.</param>
    /// <returns>Veritabanina yazilan kayit sayisi.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity<Guid>>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
                entry.Entity.UpdatedAt = utcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
            }
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        var aggregateRoots = ChangeTracker
            .Entries<AggregateRoot<Guid>>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.GetDomainEvents().Count != 0)
            .ToList();

        foreach (var aggregateRoot in aggregateRoots)
        {
            var domainEvents = aggregateRoot.GetDomainEvents().ToList();

            foreach (var domainEvent in domainEvents)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }

            aggregateRoot.ClearDomainEvents();
        }

        return result;
    }

    /// <summary>
    /// Model olusturulurken soft delete kullanan entity'ler icin global query filter ekler.
    /// </summary>
    /// <param name="modelBuilder">EF Core model konfigurasyon nesnesi.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity<Guid>).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.Property(parameter, nameof(BaseEntity<Guid>.IsDeleted));
            var comparison = Expression.Equal(property, Expression.Constant(false));
            var lambda = Expression.Lambda(comparison, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }
}

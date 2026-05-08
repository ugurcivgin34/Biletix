namespace Biletix.Domain.Base;

/// <summary>
/// Domain event ureten aggregate kokleri icin temel siniftir.
/// </summary>
/// <typeparam name="TId">Aggregate kokunun kimlik tipi.</typeparam>
public abstract class AggregateRoot<TId> : BaseEntity<TId>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Aggregate icinde gerceklesen bir domain event'i daha sonra yayinlanmak uzere listeye ekler.
    /// </summary>
    /// <param name="domainEvent">Yayinlanacak domain event.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Aggregate uzerinde bekleyen domain event'leri disaridan degistirilemeyecek sekilde dondurur.
    /// </summary>
    /// <returns>Bekleyen domain event listesi.</returns>
    public IReadOnlyList<IDomainEvent> GetDomainEvents()
    {
        return _domainEvents.AsReadOnly();
    }

    /// <summary>
    /// Basariyla yayinlanan domain event'leri temizler.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

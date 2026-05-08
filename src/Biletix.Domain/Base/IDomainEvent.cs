using MediatR;

namespace Biletix.Domain.Base;

/// <summary>
/// Domain icinde olusan ve MediatR ile yayinlanabilen olaylari isaretleyen arayuzdur.
/// </summary>
public interface IDomainEvent : INotification
{
}

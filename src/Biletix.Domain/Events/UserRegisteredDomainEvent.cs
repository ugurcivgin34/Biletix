using Biletix.Domain.Base;

namespace Biletix.Domain.Events;

/// <summary>
/// Yeni bir kullanici kaydi tamamlandiginda yayinlanan domain event'tir.
/// </summary>
/// <param name="UserId">Kayit olan kullanicinin kimligi.</param>
/// <param name="Email">Kayit olan kullanicinin e-posta adresi.</param>
public sealed record UserRegisteredDomainEvent(Guid UserId, string Email) : IDomainEvent;

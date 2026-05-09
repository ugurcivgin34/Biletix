namespace Biletix.Application.Common.Interfaces;

/// <summary>
/// Bilet tipi bazinda Redis uzerinden dagitik rezervasyon kilidi yoneten servis sozlesmesidir.
/// </summary>
public interface ITicketLockService
{
    /// <summary>
    /// Belirtilen kullanici icin bilet tipi kilidini almaya calisir.
    /// </summary>
    /// <param name="ticketTypeId">Kilitlenecek bilet tipi kimligi.</param>
    /// <param name="userId">Kilidi almak isteyen kullanici kimligi.</param>
    /// <param name="expiry">Kilidin gecerlilik suresi.</param>
    /// <returns>Kilit alindiysa true; baska bir istek tarafindan tutuluyorsa false.</returns>
    Task<bool> AcquireLockAsync(Guid ticketTypeId, Guid userId, TimeSpan expiry);

    /// <summary>
    /// Bilet tipi kilidini sadece kilidin sahibi olan kullanici icin serbest birakir.
    /// </summary>
    /// <param name="ticketTypeId">Serbest birakilacak bilet tipi kimligi.</param>
    /// <param name="userId">Kilidi birakmak isteyen kullanici kimligi.</param>
    /// <returns>Kilit silindiyse true.</returns>
    Task<bool> ReleaseLockAsync(Guid ticketTypeId, Guid userId);

    /// <summary>
    /// Bilet tipi kilidini tutan kullaniciyi dondurur.
    /// </summary>
    /// <param name="ticketTypeId">Kontrol edilecek bilet tipi kimligi.</param>
    /// <returns>Kilit sahibi kullanici kimligi; kilit yoksa null.</returns>
    Task<Guid?> GetLockOwnerAsync(Guid ticketTypeId);

    /// <summary>
    /// Kilit sahibi kullanici icin kilidin suresini uzatir.
    /// </summary>
    /// <param name="ticketTypeId">Suresi uzatilacak bilet tipi kimligi.</param>
    /// <param name="userId">Kilidin sahibi kullanici kimligi.</param>
    /// <param name="extension">Yeni kilit suresi.</param>
    /// <returns>Kilit suresi uzatildiysa true.</returns>
    Task<bool> ExtendLockAsync(Guid ticketTypeId, Guid userId, TimeSpan extension);
}

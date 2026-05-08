using Biletix.Application.Common.Interfaces;

namespace Biletix.API.Services;

/// <summary>
/// Aktif kullanicinin kaynak sahipligine gore islem yapip yapamayacagini kontrol eder.
/// </summary>
public class ResourceAuthorizationService : IResourceAuthorizationService
{
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Current user servisini alir.
    /// </summary>
    /// <param name="currentUserService">Aktif kullanici bilgilerini saglayan servis.</param>
    public ResourceAuthorizationService(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Admin kullanicilarin veya kendi etkinligini yoneten organizer'in etkinligi yonetmesine izin verir.
    /// </summary>
    /// <param name="eventOrganizerId">Etkinligin organizer kullanici kimligi.</param>
    /// <returns>Yetki varsa true.</returns>
    public bool CanManageEvent(Guid eventOrganizerId)
    {
        if (_currentUserService.IsInRole("Admin"))
        {
            return true;
        }

        return _currentUserService.IsInRole("Organizer") &&
            _currentUserService.UserId == eventOrganizerId;
    }

    /// <summary>
    /// Admin kullanicilarin veya rezervasyon sahibinin rezervasyonu yonetmesine izin verir.
    /// </summary>
    /// <param name="bookingUserId">Rezervasyon sahibi kullanici kimligi.</param>
    /// <returns>Yetki varsa true.</returns>
    public bool CanManageBooking(Guid bookingUserId)
    {
        if (_currentUserService.IsInRole("Admin"))
        {
            return true;
        }

        return _currentUserService.UserId == bookingUserId;
    }
}

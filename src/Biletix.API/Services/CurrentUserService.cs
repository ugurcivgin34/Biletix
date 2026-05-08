using System.Security.Claims;
using Biletix.Application.Common.Interfaces;

namespace Biletix.API.Services;

/// <summary>
/// HTTP context uzerindeki JWT claim'lerinden aktif kullanici bilgisini okur.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// HTTP context accessor bagimliligini alir.
    /// </summary>
    /// <param name="httpContextAccessor">Mevcut HTTP context'e erisim saglayan servis.</param>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// JWT "sub" claim'ini Guid olarak dondurur.
    /// </summary>
    public Guid? UserId
    {
        get
        {
            var value = User?.FindFirst("sub")?.Value
                ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    /// <summary>
    /// JWT e-posta claim'ini dondurur.
    /// </summary>
    public string? Email => User?.FindFirst("email")?.Value
        ?? User?.FindFirst(ClaimTypes.Email)?.Value;

    /// <summary>
    /// JWT rol claim'ini dondurur.
    /// </summary>
    public string? Role => User?.FindFirst("role")?.Value
        ?? User?.FindFirst(ClaimTypes.Role)?.Value;

    /// <summary>
    /// Mevcut kullanicinin authenticated olup olmadigini belirtir.
    /// </summary>
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    /// <summary>
    /// Mevcut kullanicinin belirtilen rolde olup olmadigini kontrol eder.
    /// </summary>
    /// <param name="role">Kontrol edilecek rol adi.</param>
    /// <returns>Kullanici belirtilen roldeyse true.</returns>
    public bool IsInRole(string role)
    {
        return User?.IsInRole(role) ?? false;
    }
}

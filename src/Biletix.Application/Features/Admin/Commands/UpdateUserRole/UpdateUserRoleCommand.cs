using Biletix.Application.Common.Interfaces;

namespace Biletix.Application.Features.Admin.Commands.UpdateUserRole;

/// <summary>
/// Bir kullanicinin rolunu admin tarafindan gunceller.
/// </summary>
public sealed class UpdateUserRoleCommand : ICommand
{
    /// <summary>
    /// Rolu guncellenecek kullanici kimligi.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Yeni rol adi.
    /// </summary>
    public string Role { get; set; } = string.Empty;
}

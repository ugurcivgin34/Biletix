using Biletix.Application.Common.Interfaces;
using Biletix.Application.Common.Models;
using Biletix.Application.Features.Admin.DTOs;

namespace Biletix.Application.Features.Admin.Queries.GetUsers;

/// <summary>
/// Admin paneli icin kullanicilari sayfali olarak listeler.
/// </summary>
public sealed class GetUsersQuery : IQuery<PagedResult<UserResponse>>
{
    /// <summary>
    /// Opsiyonel rol filtresi.
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Sayfa numarasi.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Sayfa boyutu.
    /// </summary>
    public int PageSize { get; set; } = 20;
}

using Biletix.Application.Common.Interfaces;
using Biletix.Application.Common.Models;
using Biletix.Application.Features.Performers.DTOs;

namespace Biletix.Application.Features.Performers.Queries.GetPerformers;

/// <summary>
/// Performer listelemek icin kullanilan sorgudur.
/// </summary>
public sealed class GetPerformersQuery : IQuery<PagedResult<PerformerResponse>>
{
    /// <summary>
    /// Performer adinda aranacak opsiyonel metin.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Sayfa numarasi.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Sayfa basina kayit sayisi.
    /// </summary>
    public int PageSize { get; set; } = 20;
}

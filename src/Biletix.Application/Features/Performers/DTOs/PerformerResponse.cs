namespace Biletix.Application.Features.Performers.DTOs;

/// <summary>
/// Performer listeleme endpoint'lerinde dondurulen cevap modelidir.
/// </summary>
/// <param name="Id">Performer benzersiz kimligi.</param>
/// <param name="Name">Performer gorunen adi.</param>
/// <param name="Genre">Performer turu veya janri.</param>
/// <param name="ImageUrl">Opsiyonel performer gorsel adresi.</param>
public sealed record PerformerResponse(
    Guid Id,
    string Name,
    string Genre,
    string? ImageUrl);

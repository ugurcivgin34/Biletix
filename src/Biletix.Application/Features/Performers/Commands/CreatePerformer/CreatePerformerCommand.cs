using Biletix.Application.Common.Interfaces;

namespace Biletix.Application.Features.Performers.Commands.CreatePerformer;

/// <summary>
/// Yeni bir performer olusturmak icin kullanilan komuttur.
/// </summary>
public sealed class CreatePerformerCommand : ICommand<Guid>
{
    /// <summary>
    /// Performer gorunen adi.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Performer turu veya janri.
    /// </summary>
    public string Genre { get; set; } = string.Empty;

    /// <summary>
    /// Opsiyonel performer gorsel adresi.
    /// </summary>
    public string? ImageUrl { get; set; }
}

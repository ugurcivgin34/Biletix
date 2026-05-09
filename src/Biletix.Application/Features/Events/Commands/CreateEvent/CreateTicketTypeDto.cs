namespace Biletix.Application.Features.Events.Commands.CreateEvent;

/// <summary>
/// Etkinlik olusturulurken beraber tanimlanan bilet tipi bilgisidir.
/// </summary>
/// <param name="Name">Bilet tipinin gorunen adi.</param>
/// <param name="Price">Bilet tipinin birim fiyati.</param>
/// <param name="TotalCapacity">Bilet tipi icin toplam kapasite.</param>
public sealed record CreateTicketTypeDto(string Name, decimal Price, int TotalCapacity);

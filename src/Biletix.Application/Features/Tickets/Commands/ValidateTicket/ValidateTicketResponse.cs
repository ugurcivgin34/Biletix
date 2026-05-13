namespace Biletix.Application.Features.Tickets.Commands.ValidateTicket;

/// <summary>
/// QR bilet dogrulama sonucunu API'ye tasiyan response modelidir.
/// </summary>
public sealed record ValidateTicketResponse(
    bool IsValid,
    string Message,
    Guid? BookingId,
    Guid? EventId,
    Guid? UserId,
    string? AttendeeFirstName,
    string? AttendeeLastName,
    string? EventTitle,
    DateTime? EventStartDate,
    bool AlreadyScanned,
    DateTime? FirstScannedAt);

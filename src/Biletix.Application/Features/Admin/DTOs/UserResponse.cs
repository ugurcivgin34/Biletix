namespace Biletix.Application.Features.Admin.DTOs;

/// <summary>
/// Admin kullanici listesi icin kullanici ozet cevabidir.
/// </summary>
public sealed record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsActive,
    DateTime CreatedAt);

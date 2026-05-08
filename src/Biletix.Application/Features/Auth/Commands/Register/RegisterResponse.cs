namespace Biletix.Application.Features.Auth.Commands.Register;

/// <summary>
/// Basarili kullanici kaydi sonrasi dondurulen cevap modelidir.
/// </summary>
/// <param name="UserId">Olusturulan kullanicinin kimligi.</param>
/// <param name="Email">Kullanicinin e-posta adresi.</param>
/// <param name="FirstName">Kullanicinin adi.</param>
/// <param name="LastName">Kullanicinin soyadi.</param>
public sealed record RegisterResponse(Guid UserId, string Email, string FirstName, string LastName);

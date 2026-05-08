using System.Net.Mail;
using Biletix.Domain.Base;
using Biletix.Domain.Events;
using Biletix.Domain.Exceptions;

namespace Biletix.Domain.Entities;

/// <summary>
/// Biletix sistemindeki kimlik dogrulama ve yetkilendirme kullanicisini temsil eder.
/// </summary>
public class User : AggregateRoot<Guid>
{
    private User()
    {
    }

    /// <summary>
    /// Kullanici e-posta adresidir; normalize edilmis kucuk harf formatinda saklanir.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// BCrypt ile hashlenmis sifre degeridir.
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// Kullanicinin adidir.
    /// </summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>
    /// Kullanicinin soyadidir.
    /// </summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>
    /// Kullanicinin sistemdeki roludur.
    /// </summary>
    public UserRole Role { get; private set; }

    /// <summary>
    /// Kullanici hesabinin aktif olup olmadigini belirtir.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Kullanicinin son basarili giris zamanidir.
    /// </summary>
    public DateTime? LastLoginAt { get; private set; }

    /// <summary>
    /// Gecerli bilgilerle varsayilan Customer rolunde aktif bir kullanici olusturur.
    /// </summary>
    /// <param name="email">Kullanicinin e-posta adresi.</param>
    /// <param name="firstName">Kullanicinin adi.</param>
    /// <param name="lastName">Kullanicinin soyadi.</param>
    /// <param name="passwordHash">Hashlenmis sifre.</param>
    /// <returns>Yeni kullanici aggregate'i.</returns>
    /// <exception cref="DomainException">Zorunlu alanlar bos veya e-posta formati gecersizse firlatilir.</exception>
    public static User Create(string email, string firstName, string lastName, string passwordHash)
    {
        var normalizedEmail = NormalizeAndValidateEmail(email);

        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new DomainException("First name cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new DomainException("Last name cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("Password hash cannot be empty");
        }

        var utcNow = DateTime.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            PasswordHash = passwordHash,
            Role = UserRole.Customer,
            IsActive = true,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        user.AddDomainEvent(new UserRegisteredDomainEvent(user.Id, user.Email));

        return user;
    }

    /// <summary>
    /// Gecerli bilgilerle Admin rolunde aktif bir kullanici olusturur.
    /// </summary>
    /// <param name="email">Admin kullanicinin e-posta adresi.</param>
    /// <param name="firstName">Admin kullanicinin adi.</param>
    /// <param name="lastName">Admin kullanicinin soyadi.</param>
    /// <param name="passwordHash">Hashlenmis sifre.</param>
    /// <returns>Admin rolundeki yeni kullanici aggregate'i.</returns>
    public static User CreateAdmin(string email, string firstName, string lastName, string passwordHash)
    {
        var user = Create(email, firstName, lastName, passwordHash);
        user.Role = UserRole.Admin;

        return user;
    }

    /// <summary>
    /// Son basarili giris zamanini gunceller.
    /// </summary>
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Kullanici hesabini pasif hale getirir.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string NormalizeAndValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("Email cannot be empty");
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();

        try
        {
            var mailAddress = new MailAddress(normalizedEmail);

            if (!string.Equals(mailAddress.Address, normalizedEmail, StringComparison.OrdinalIgnoreCase))
            {
                throw new DomainException("Email format is invalid");
            }
        }
        catch (FormatException)
        {
            throw new DomainException("Email format is invalid");
        }

        return normalizedEmail;
    }
}

namespace Biletix.Domain.Exceptions;

/// <summary>
/// Domain kurallari ihlal edildiginde firlatilan genel domain istisnasidir.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Domain kural ihlalini aciklayan mesaj ile istisna olusturur.
    /// </summary>
    /// <param name="message">Kural ihlalinin aciklamasi.</param>
    public DomainException(string message)
        : base(message)
    {
    }
}

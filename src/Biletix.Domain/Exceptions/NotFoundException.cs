namespace Biletix.Domain.Exceptions;

/// <summary>
/// Istenen domain kaydi bulunamadiginda firlatilan istisnadir.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Entity adi ve aranan anahtar bilgisinden standart bir bulunamadi mesaji olusturur.
    /// </summary>
    /// <param name="entityName">Aranan entity'nin adi.</param>
    /// <param name="key">Aranan kaydin anahtari.</param>
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.")
    {
    }
}

namespace Biletix.Application.Common.Models;

/// <summary>
/// Bir islemin basarili olup olmadigini, basariliysa degerini, basarisizsa hata mesajini tasir.
/// </summary>
/// <typeparam name="T">Basarili sonuc degerinin tipi.</typeparam>
public class Result<T>
{
    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    /// <summary>
    /// Islemin basarili tamamlanip tamamlanmadigini belirtir.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Islem basariliysa donen degeri tasir.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Islem basarisizsa hata aciklamasini tasir.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Basarili ve deger tasiyan bir sonuc olusturur.
    /// </summary>
    /// <param name="value">Sonuc degeri.</param>
    /// <returns>Basarili sonuc nesnesi.</returns>
    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null);
    }

    /// <summary>
    /// Basarisiz ve hata mesaji tasiyan bir sonuc olusturur.
    /// </summary>
    /// <param name="error">Hata mesaji.</param>
    /// <returns>Basarisiz sonuc nesnesi.</returns>
    public static Result<T> Failure(string error)
    {
        return new Result<T>(false, default, error);
    }
}

/// <summary>
/// Deger dondurmeyen islemler icin basari veya hata bilgisini tasir.
/// </summary>
public class Result
{
    private Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Islemin basarili tamamlanip tamamlanmadigini belirtir.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Islem basarisizsa hata aciklamasini tasir.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Basarili ve deger tasimayan bir sonuc olusturur.
    /// </summary>
    /// <returns>Basarili sonuc nesnesi.</returns>
    public static Result Success()
    {
        return new Result(true, null);
    }

    /// <summary>
    /// Basarisiz ve hata mesaji tasiyan bir sonuc olusturur.
    /// </summary>
    /// <param name="error">Hata mesaji.</param>
    /// <returns>Basarisiz sonuc nesnesi.</returns>
    public static Result Failure(string error)
    {
        return new Result(false, error);
    }
}

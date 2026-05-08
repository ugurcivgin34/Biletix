namespace Biletix.Application.Common.Models;

/// <summary>
/// Sayfalanmis liste cevaplari icin veri, toplam kayit ve sayfa bilgilerini tasir.
/// </summary>
/// <typeparam name="T">Listedeki eleman tipi.</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Sayfalanmis sonuc nesnesini olusturur.
    /// </summary>
    /// <param name="items">Mevcut sayfadaki elemanlar.</param>
    /// <param name="totalCount">Tum filtrelere gore toplam kayit sayisi.</param>
    /// <param name="page">Mevcut sayfa numarasi.</param>
    /// <param name="pageSize">Bir sayfada yer alan maksimum eleman sayisi.</param>
    public PagedResult(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }

    /// <summary>
    /// Mevcut sayfada dondurulen elemanlar.
    /// </summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>
    /// Tum sayfalardaki toplam kayit sayisi.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Mevcut sayfa numarasi.
    /// </summary>
    public int Page { get; }

    /// <summary>
    /// Bir sayfada istenen kayit sayisi.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Toplam kayit ve sayfa boyutuna gore hesaplanan toplam sayfa sayisi.
    /// </summary>
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Mevcut sayfadan sonra baska sayfa olup olmadigini belirtir.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Mevcut sayfadan once baska sayfa olup olmadigini belirtir.
    /// </summary>
    public bool HasPreviousPage => Page > 1;
}

namespace Biletix.API.Common;

/// <summary>
/// Minimal API endpoint gruplarini moduler siniflar halinde tanimlamak icin kullanilan sozlesmedir.
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Endpoint route'larini uygulamanin route builder'i uzerine map eder.
    /// </summary>
    /// <param name="app">Endpoint'lerin eklenecegi route builder.</param>
    void MapEndpoint(IEndpointRouteBuilder app);
}

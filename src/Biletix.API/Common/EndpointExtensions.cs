using System.Reflection;

namespace Biletix.API.Common;

/// <summary>
/// IEndpoint implementasyonlarini assembly icinden bulup Minimal API route'larina ekleyen extension metotlari icerir.
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Verilen assembly icindeki tum somut IEndpoint siniflarini olusturur ve endpoint'lerini map eder.
    /// </summary>
    /// <param name="app">Endpoint'lerin eklenecegi route builder.</param>
    /// <param name="assembly">Endpoint siniflari icin taranacak assembly.</param>
    public static void MapEndpoints(this IEndpointRouteBuilder app, Assembly assembly)
    {
        var endpointTypes = assembly
            .GetTypes()
            .Where(type =>
                type is { IsAbstract: false, IsInterface: false } &&
                typeof(IEndpoint).IsAssignableFrom(type));

        foreach (var endpointType in endpointTypes)
        {
            if (Activator.CreateInstance(endpointType) is IEndpoint endpoint)
            {
                endpoint.MapEndpoint(app);
            }
        }
    }
}

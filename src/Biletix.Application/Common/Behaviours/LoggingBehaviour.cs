using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Biletix.Application.Common.Behaviours;

/// <summary>
/// MediatR pipeline'inda her istegin baslangicini, bitisini ve calisma suresini loglar.
/// </summary>
/// <typeparam name="TRequest">Pipeline'dan gecen request tipi.</typeparam>
/// <typeparam name="TResponse">Request sonucunda donen response tipi.</typeparam>
/// <remarks>
/// Logging behavior icin gerekli logger bagimliligini alir.
/// </remarks>
/// <param name="logger">Request loglarini yazacak logger.</param>
public class LoggingBehaviour<TRequest, TResponse>(ILogger<LoggingBehaviour<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{

    /// <summary>
    /// Request'i bir sonraki pipeline adimina iletirken calisma suresini olcer ve loglar.
    /// </summary>
    /// <param name="request">Islenen MediatR request'i.</param>
    /// <param name="next">Pipeline'daki sonraki adim veya asil handler.</param>
    /// <param name="cancellationToken">Asenkron islemi iptal etmek icin kullanilan token.</param>
    /// <returns>Pipeline sonucunda uretilen response.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = request.GetType().Name;

        logger.LogInformation("Handling request: {Name}", requestName);

        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        logger.LogInformation("Handled request: {Name} ({Elapsed}ms)", requestName, elapsedMilliseconds);

        if (elapsedMilliseconds > 500)
        {
            logger.LogWarning("Long running request: {Name} ({Elapsed}ms)", requestName, elapsedMilliseconds);
        }

        return response;
    }
}

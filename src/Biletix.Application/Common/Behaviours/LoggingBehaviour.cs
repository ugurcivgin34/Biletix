using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Biletix.Application.Common.Behaviours;

/// <summary>
/// MediatR pipeline'inda her istegin baslangicini, bitisini ve calisma suresini loglar.
/// </summary>
/// <typeparam name="TRequest">Pipeline'dan gecen request tipi.</typeparam>
/// <typeparam name="TResponse">Request sonucunda donen response tipi.</typeparam>
public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    /// <summary>
    /// Logging behavior icin gerekli logger bagimliligini alir.
    /// </summary>
    /// <param name="logger">Request loglarini yazacak logger.</param>
    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

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

        _logger.LogInformation("Handling request: {Name}", requestName);

        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        _logger.LogInformation("Handled request: {Name} ({Elapsed}ms)", requestName, elapsedMilliseconds);

        if (elapsedMilliseconds > 500)
        {
            _logger.LogWarning("Long running request: {Name} ({Elapsed}ms)", requestName, elapsedMilliseconds);
        }

        return response;
    }
}

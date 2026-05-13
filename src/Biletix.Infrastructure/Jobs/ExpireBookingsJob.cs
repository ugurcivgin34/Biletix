using Biletix.Application.Features.Bookings.Commands.ExpireBookings;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Biletix.Infrastructure.Jobs;

/// <summary>
/// Suresi dolan pending rezervasyonlari periyodik olarak expire eden background job'dir.
/// </summary>
public sealed class ExpireBookingsJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpireBookingsJob> _logger;

    /// <summary>
    /// Job'un ihtiyac duydugu scope factory ve logger servislerini alir.
    /// </summary>
    /// <param name="scopeFactory">Scoped servisleri job calismasinda olusturmak icin kullanilir.</param>
    /// <param name="logger">Job loglarini yazan logger.</param>
    public ExpireBookingsJob(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpireBookingsJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Her 60 saniyede bir expire command'ini calistirir.
    /// </summary>
    /// <param name="stoppingToken">Servis durdurma bildirimi.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                var count = await sender.Send(new ExpireBookingsCommand(), stoppingToken);

                if (count > 0)
                {
                    _logger.LogInformation(
                        "ExpireBookingsJob: expired {Count} bookings",
                        count);
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "ExpireBookingsJob error");
            }
        }
    }
}

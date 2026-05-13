using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Queue.DTOs;
using Biletix.Domain.Exceptions;

namespace Biletix.Application.Features.Queue.Queries.GetQueueStatus;

/// <summary>
/// Aktif kullanicinin etkinlik bekleme sirasi durumunu getiren sorgu handler'idir.
/// </summary>
public sealed class GetQueueStatusQueryHandler : IQueryHandler<GetQueueStatusQuery, QueueStatusResponse>
{
    private readonly IWaitingQueueService _waitingQueueService;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Handler'in ihtiyac duydugu sira ve kullanici servislerini alir.
    /// </summary>
    /// <param name="waitingQueueService">Bekleme sirasi servisi.</param>
    /// <param name="currentUserService">Aktif kullanici servisi.</param>
    public GetQueueStatusQueryHandler(
        IWaitingQueueService waitingQueueService,
        ICurrentUserService currentUserService)
    {
        _waitingQueueService = waitingQueueService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Kullanicinin siradaki pozisyonunu ve devam edebilme durumunu dondurur.
    /// </summary>
    /// <param name="request">Sira durumu sorgusu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    /// <returns>Guncel sira durumu.</returns>
    public async Task<QueueStatusResponse> Handle(
        GetQueueStatusQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new DomainException("Authenticated user is required");

        var position = await _waitingQueueService.GetPositionAsync(request.EventId, userId);
        var queueLength = await _waitingQueueService.GetQueueLengthAsync(request.EventId);

        if (!position.HasValue)
        {
            return new QueueStatusResponse(
                request.EventId,
                userId,
                0,
                queueLength,
                false,
                0,
                false);
        }

        var canProceed = await _waitingQueueService.CanProceedAsync(request.EventId, userId);
        var estimatedWaitSeconds = await _waitingQueueService.GetEstimatedWaitTimeAsync(request.EventId, userId);

        return new QueueStatusResponse(
            request.EventId,
            userId,
            position.Value,
            queueLength,
            canProceed,
            estimatedWaitSeconds,
            true);
    }
}

using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Queue.DTOs;
using Biletix.Domain.Exceptions;

namespace Biletix.Application.Features.Queue.Commands.JoinQueue;

/// <summary>
/// Aktif kullaniciyi etkinlik bekleme sirasina ekleyen komut handler'idir.
/// </summary>
public sealed class JoinQueueCommandHandler : ICommandHandler<JoinQueueCommand, QueueStatusResponse>
{
    private readonly IWaitingQueueService _waitingQueueService;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Handler'in ihtiyac duydugu sira ve kullanici servislerini alir.
    /// </summary>
    /// <param name="waitingQueueService">Bekleme sirasi servisi.</param>
    /// <param name="currentUserService">Aktif kullanici servisi.</param>
    public JoinQueueCommandHandler(
        IWaitingQueueService waitingQueueService,
        ICurrentUserService currentUserService)
    {
        _waitingQueueService = waitingQueueService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Kullaniciyi siraya ekler ve guncel sira durumunu dondurur.
    /// </summary>
    /// <param name="request">Siraya katilma komutu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    /// <returns>Guncel sira durumu.</returns>
    public async Task<QueueStatusResponse> Handle(
        JoinQueueCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new DomainException("Authenticated user is required");

        var position = await _waitingQueueService.EnqueueAsync(request.EventId, userId);
        var queueLength = await _waitingQueueService.GetQueueLengthAsync(request.EventId);
        var canProceed = await _waitingQueueService.CanProceedAsync(request.EventId, userId);
        var estimatedWaitSeconds = await _waitingQueueService.GetEstimatedWaitTimeAsync(request.EventId, userId);

        return new QueueStatusResponse(
            request.EventId,
            userId,
            position,
            queueLength,
            canProceed,
            estimatedWaitSeconds,
            true);
    }
}

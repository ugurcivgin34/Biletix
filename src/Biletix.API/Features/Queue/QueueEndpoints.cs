using System.Text.Json;
using Biletix.API.Common;
using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Queue.Commands.JoinQueue;
using Biletix.Application.Features.Queue.DTOs;
using Biletix.Application.Features.Queue.Queries.GetQueueStatus;
using MediatR;

namespace Biletix.API.Features.Queue;

/// <summary>
/// Sanal bekleme sirasi endpoint'lerini Minimal API uzerinden map eder.
/// </summary>
public sealed class QueueEndpoints : IEndpoint
{
    /// <summary>
    /// Siraya katilma, durum sorgulama, siradan ayrilma ve SSE stream route'larini tanimlar.
    /// </summary>
    /// <param name="app">Endpoint'lerin eklenecegi route builder.</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/queue")
            .WithTags("Queue")
            .RequireAuthorization("AuthenticatedUser");

        group.MapPost("/{eventId:guid}/join", JoinQueueAsync)
            .WithName("JoinQueue")
            .Produces<QueueStatusResponse>(StatusCodes.Status200OK);

        group.MapGet("/{eventId:guid}/status", GetQueueStatusAsync)
            .WithName("GetQueueStatus")
            .Produces<QueueStatusResponse>(StatusCodes.Status200OK);

        group.MapDelete("/{eventId:guid}/leave", LeaveQueueAsync)
            .WithName("LeaveQueue")
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/{eventId:guid}/stream", StreamQueueAsync)
            .WithName("StreamQueue")
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> JoinQueueAsync(Guid eventId, ISender sender)
    {
        var response = await sender.Send(new JoinQueueCommand { EventId = eventId });
        return Results.Ok(response);
    }

    private static async Task<IResult> GetQueueStatusAsync(Guid eventId, ISender sender)
    {
        var response = await sender.Send(new GetQueueStatusQuery { EventId = eventId });
        return Results.Ok(response);
    }

    private static async Task<IResult> LeaveQueueAsync(
        Guid eventId,
        IWaitingQueueService queueService,
        ICurrentUserService currentUserService)
    {
        if (currentUserService.UserId is not { } userId)
        {
            return Results.Unauthorized();
        }

        await queueService.DequeueAsync(eventId, userId);
        return Results.NoContent();
    }

    private static async Task StreamQueueAsync(
        Guid eventId,
        HttpContext httpContext,
        IWaitingQueueService queueService,
        ICurrentUserService currentUserService,
        CancellationToken ct)
    {
        httpContext.Response.Headers.Append("Content-Type", "text/event-stream");
        httpContext.Response.Headers.Append("Cache-Control", "no-cache");
        httpContext.Response.Headers.Append("X-Accel-Buffering", "no");

        if (currentUserService.UserId is not { } userId)
        {
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        while (!ct.IsCancellationRequested)
        {
            var position = await queueService.GetPositionAsync(eventId, userId);
            var queueLength = await queueService.GetQueueLengthAsync(eventId);
            var canProceed = await queueService.CanProceedAsync(eventId, userId);
            var waitSeconds = await queueService.GetEstimatedWaitTimeAsync(eventId, userId);

            var data = new
            {
                position = position ?? 0,
                totalInQueue = queueLength,
                canProceed,
                estimatedWaitSeconds = waitSeconds,
                timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(data);
            await httpContext.Response.WriteAsync($"data: {json}\n\n", ct);
            await httpContext.Response.Body.FlushAsync(ct);

            if (canProceed)
            {
                await httpContext.Response.WriteAsync("event: proceed\ndata: go\n\n", ct);
                await httpContext.Response.Body.FlushAsync(ct);
                break;
            }

            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }
    }
}

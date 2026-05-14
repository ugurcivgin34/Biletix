using Biletix.API.Common;
using Biletix.Application.Common.Models;
using Biletix.Application.Features.Admin.Commands.UpdateUserRole;
using Biletix.Application.Features.Admin.DTOs;
using Biletix.Application.Features.Admin.Queries.GetUsers;
using MediatR;

namespace Biletix.API.Features.Admin;

/// <summary>
/// Admin paneli endpoint'lerini Minimal API uzerinden map eder.
/// </summary>
public sealed class AdminEndpoints : IEndpoint
{
    /// <summary>
    /// Admin kullanici yonetimi route'larini tanimlar.
    /// </summary>
    /// <param name="app">Endpoint'lerin eklenecegi route builder.</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization("AdminOnly");

        group.MapGet("/users", GetUsersAsync)
            .WithName("AdminGetUsers")
            .Produces<PagedResult<UserResponse>>(StatusCodes.Status200OK);

        group.MapPatch("/users/{userId:guid}/role", UpdateUserRoleAsync)
            .WithName("AdminUpdateUserRole")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> GetUsersAsync(
        ISender sender,
        string? role = null,
        int page = 1,
        int pageSize = 20)
    {
        var response = await sender.Send(new GetUsersQuery
        {
            Role = role,
            Page = page,
            PageSize = pageSize
        });

        return Results.Ok(response);
    }

    private static async Task<IResult> UpdateUserRoleAsync(
        Guid userId,
        UpdateUserRoleRequest request,
        ISender sender,
        CancellationToken ct)
    {
        await sender.Send(new UpdateUserRoleCommand
        {
            UserId = userId,
            Role = request.Role
        }, ct);

        return Results.NoContent();
    }
}

/// <summary>
/// Kullanici rol guncelleme request modelidir.
/// </summary>
/// <param name="Role">Yeni rol adi.</param>
public sealed record UpdateUserRoleRequest(string Role);

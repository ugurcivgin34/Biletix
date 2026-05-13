using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Biletix.API.Common;
using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Auth.Commands.Login;
using Biletix.Application.Features.Auth.Commands.Logout;
using Biletix.Application.Features.Auth.Commands.RefreshToken;
using Biletix.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace Biletix.API.Features.Auth;

/// <summary>
/// Kimlik dogrulama endpoint'lerini Minimal API uzerinden map eder.
/// </summary>
public class AuthEndpoints : IEndpoint
{
    /// <summary>
    /// Auth route grubunu ve register, login, refresh, logout endpoint'lerini tanimlar.
    /// </summary>
    /// <param name="app">Endpoint'lerin eklenecegi route builder.</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/register", RegisterAsync)
            .AllowAnonymous()
            .RequireRateLimiting("auth")
            .WithName("Register")
            .Produces<RegisterResponse>(StatusCodes.Status201Created);

        group.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .RequireRateLimiting("auth")
            .WithName("Login")
            .Produces<LoginResponse>(StatusCodes.Status200OK);

        group.MapPost("/refresh", RefreshAsync)
            .AllowAnonymous()
            .RequireRateLimiting("auth")
            .WithName("RefreshToken")
            .Produces<LoginResponse>(StatusCodes.Status200OK);

        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .WithName("Logout")
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/me", Me)
            .RequireAuthorization()
            .WithName("Me")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/admin-only", AdminOnly)
            .RequireAuthorization("AdminOnly")
            .WithName("AdminOnly")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/organizer-panel", OrganizerPanel)
            .RequireAuthorization("OrganizerOrAdmin")
            .WithName("OrganizerPanel")
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> RegisterAsync(RegisterRequest request, ISender sender)
    {
        var response = await sender.Send(new RegisterCommand
        {
            Email = request.Email,
            Password = request.Password,
            FirstName = request.FirstName,
            LastName = request.LastName
        });

        return Results.Created($"/api/auth/users/{response.UserId}", response);
    }

    private static async Task<IResult> LoginAsync(LoginRequest request, ISender sender)
    {
        var response = await sender.Send(new LoginCommand
        {
            Email = request.Email,
            Password = request.Password
        });

        return Results.Ok(response);
    }

    private static async Task<IResult> RefreshAsync(RefreshTokenRequest request, ISender sender)
    {
        var response = await sender.Send(new RefreshTokenCommand
        {
            AccessToken = request.AccessToken,
            RefreshToken = request.RefreshToken
        });

        return Results.Ok(response);
    }

    private static IResult Me(ICurrentUserService currentUserService)
    {
        return Results.Ok(new
        {
            userId = currentUserService.UserId,
            email = currentUserService.Email,
            role = currentUserService.Role,
            isAuthenticated = currentUserService.IsAuthenticated
        });
    }

    private static IResult AdminOnly()
    {
        return Results.Ok(new { message = "Welcome Admin" });
    }

    private static IResult OrganizerPanel()
    {
        return Results.Ok(new { message = "Welcome to Organizer Panel" });
    }

    [Authorize]
    private static async Task<IResult> LogoutAsync(
        LogoutRequest request,
        ClaimsPrincipal user,
        ISender sender)
    {
        var userIdValue = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Results.Unauthorized();
        }

        await sender.Send(new LogoutCommand
        {
            UserId = userId,
            RefreshToken = request.RefreshToken
        });

        return Results.NoContent();
    }
}

/// <summary>
/// Kullanici kayit endpoint'i icin request modelidir.
/// </summary>
/// <param name="Email">Kayit olacak kullanicinin e-posta adresi.</param>
/// <param name="Password">Kayit olacak kullanicinin sifresi.</param>
/// <param name="FirstName">Kayit olacak kullanicinin adi.</param>
/// <param name="LastName">Kayit olacak kullanicinin soyadi.</param>
public sealed record RegisterRequest(string Email, string Password, string FirstName, string LastName);

/// <summary>
/// Kullanici giris endpoint'i icin request modelidir.
/// </summary>
/// <param name="Email">Giris yapacak kullanicinin e-posta adresi.</param>
/// <param name="Password">Giris yapacak kullanicinin sifresi.</param>
public sealed record LoginRequest(string Email, string Password);

/// <summary>
/// Refresh token endpoint'i icin request modelidir.
/// </summary>
/// <param name="AccessToken">Suresi dolmus veya yenilenecek access token.</param>
/// <param name="RefreshToken">Mevcut refresh token.</param>
public sealed record RefreshTokenRequest(string AccessToken, string RefreshToken);

/// <summary>
/// Logout endpoint'i icin request modelidir.
/// </summary>
/// <param name="RefreshToken">Iptal edilecek refresh token.</param>
public sealed record LogoutRequest(string RefreshToken);

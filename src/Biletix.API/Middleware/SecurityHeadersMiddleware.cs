namespace Biletix.API.Middleware;

/// <summary>
/// Tum HTTP response'lara temel tarayici guvenlik header'larini ekler.
/// </summary>
public sealed class SecurityHeadersMiddleware : IMiddleware
{
    /// <summary>
    /// Guvenlik header'larini set eder ve istegi pipeline'daki sonraki middleware'e aktarir.
    /// </summary>
    /// <param name="context">Islenen HTTP isteginin context nesnesi.</param>
    /// <param name="next">Pipeline'daki sonraki middleware.</param>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        await next(context);
    }
}

using Biletix.Application.Common.Interfaces;
using Biletix.Domain.Entities;
using Biletix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Infrastructure.Persistence.Seeders;

/// <summary>
/// Local ve ilk kurulum senaryolari icin varsayilan admin kullanicisini olusturur.
/// </summary>
public static class AdminSeeder
{
    /// <summary>
    /// Sistemde admin yoksa varsayilan admin hesabini olusturur.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    /// <param name="authService">Admin sifresini hashlemek icin kullanilan auth servisi.</param>
    public static async Task SeedAsync(ApplicationDbContext context, IAuthService authService)
    {
        if (await context.Users.AnyAsync(user => user.Role == UserRole.Admin))
        {
            return;
        }

        var passwordHash = await authService.HashPasswordAsync("Admin123!");
        var admin = User.CreateAdmin(
            email: "admin@biletix.com",
            firstName: "Biletix",
            lastName: "Admin",
            passwordHash: passwordHash);

        await context.Users.AddAsync(admin);
        await context.SaveChangesAsync();
    }
}

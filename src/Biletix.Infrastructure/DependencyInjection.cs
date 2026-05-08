using Biletix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Biletix.Infrastructure;

/// <summary>
/// Infrastructure katmaninin servis kayitlarini tek noktadan yapmak icin kullanilir.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// PostgreSQL kullanan ApplicationDbContext kaydini DI container'a ekler.
    /// </summary>
    /// <param name="services">Servislerin kaydedilecegi DI koleksiyonu.</param>
    /// <param name="configuration">Connection string gibi ayarlari tasiyan konfigurasyon.</param>
    /// <returns>Kayitlar eklenmis servis koleksiyonu.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }
}

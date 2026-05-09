using Biletix.Application.Common.Interfaces;
using Biletix.Infrastructure.Auth;
using Biletix.Infrastructure.Messaging.Consumers;
using Biletix.Infrastructure.Persistence;
using Biletix.Infrastructure.Search;
using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

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
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        var elasticsearchUrl = configuration["Elasticsearch:Url"]!;
        var elasticsearchSettings = new ElasticsearchClientSettings(new Uri(elasticsearchUrl))
            .DefaultIndex("biletix-events");

        services.AddSingleton(new ElasticsearchClient(elasticsearchSettings));
        services.AddSingleton<ElasticsearchIndexInitializer>();
        services.AddScoped<IEventSearchService, ElasticsearchEventSearchService>();
        services.AddHostedService<EventCdcConsumer>();

        return services;
    }

    /// <summary>
    /// Infrastructure bagimliliklari icin detayli health check kayitlarini ekler.
    /// </summary>
    /// <param name="builder">Health check kayitlarinin eklenecegi builder.</param>
    /// <param name="configuration">Connection string ve endpoint ayarlarini tasiyan konfigurasyon.</param>
    /// <returns>Kayitlar eklenmis health check builder.</returns>
    public static IHealthChecksBuilder AddInfrastructureHealthChecks(
        this IHealthChecksBuilder builder,
        IConfiguration configuration)
    {
        builder.AddNpgSql(
            configuration.GetConnectionString("DefaultConnection")!,
            name: "postgresql",
            tags: new[] { "db", "sql" });

        builder.AddRedis(
            configuration.GetConnectionString("Redis")!,
            name: "redis",
            tags: new[] { "cache" });

        builder.AddElasticsearch(
            configuration["Elasticsearch:Url"]!,
            name: "elasticsearch",
            tags: new[] { "search" });

        return builder;
    }
}

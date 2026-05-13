using System.Reflection;
using Biletix.Application.Common.Behaviours;
using Biletix.Application.Features.Bookings.Saga;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Biletix.Application;

/// <summary>
/// Application katmaninin servis kayitlarini tek noktadan yapmak icin kullanilir.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// MediatR handler'larini, FluentValidation validator'larini ve pipeline behavior'larini DI container'a ekler.
    /// </summary>
    /// <param name="services">Servislerin kaydedilecegi DI koleksiyonu.</param>
    /// <returns>Kayitlar eklenmis servis koleksiyonu.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddScoped<IBookingSaga, BookingSaga>();

        return services;
    }
}

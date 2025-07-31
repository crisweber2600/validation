using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Validation.Infrastructure.DI;

public static class ManualValidatorExtensions
{
    public static IServiceCollection AddValidatorRule<T>(this IServiceCollection services, Func<T, bool> rule)
    {
        services.TryAddSingleton<IManualValidatorService, ManualValidatorService>();
        services.AddSingleton(new ManualValidationRule(typeof(T), o => rule((T)o)));
        return services;
    }
}

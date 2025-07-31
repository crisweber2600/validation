using Microsoft.Extensions.DependencyInjection;

namespace Validation.Infrastructure.DI;

public static class ManualValidationExtensions
{
    public static IServiceCollection AddValidatorRule<T>(this IServiceCollection services, Func<T, bool> rule)
    {
        services.AddSingleton<IManualValidatorService, ManualValidatorService>();
        services.AddOptions<ManualValidatorOptions>().Configure(options =>
        {
            var list = options.Rules.GetOrAdd(typeof(T), _ => new List<Func<object, bool>>());
            list.Add(o => rule((T)o));
        });
        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace Validation.Infrastructure.DI;

public class ValidationFlowBuilder
{
    public IServiceCollection Services { get; }

    public ValidationFlowBuilder(IServiceCollection services)
    {
        Services = services;
    }
}

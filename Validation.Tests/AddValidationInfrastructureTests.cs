using Microsoft.Extensions.DependencyInjection;
using Validation.Domain;
using Validation.Infrastructure.DI;
using Validation.Infrastructure;

namespace Validation.Tests;

public class AddValidationInfrastructureTests
{
    [Fact]
    public void AddValidationInfrastructure_registers_providers()
    {
        var services = new ServiceCollection();
        services.AddValidationInfrastructure();

        using var provider = services.BuildServiceProvider();
        var idProvider = provider.GetService<IEntityIdProvider>();
        var appProvider = provider.GetService<IApplicationNameProvider>();

        Assert.NotNull(idProvider);
        Assert.NotNull(appProvider);
        Assert.IsType<ReflectionBasedEntityIdProvider>(idProvider);
        Assert.IsType<StaticApplicationNameProvider>(appProvider);
    }
}

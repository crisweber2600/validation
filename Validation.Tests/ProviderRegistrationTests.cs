using Microsoft.Extensions.DependencyInjection;
using Validation.Infrastructure;
using Validation.Infrastructure.DI;

namespace Validation.Tests;

public class ProviderRegistrationTests
{
    [Fact]
    public void AddReflectionBasedEntityIdProvider_registers_provider()
    {
        var services = new ServiceCollection();
        services.AddReflectionBasedEntityIdProvider();

        using var provider = services.BuildServiceProvider();
        var instance = provider.GetService<IEntityIdProvider>();

        Assert.NotNull(instance);
        Assert.IsType<ReflectionBasedEntityIdProvider>(instance);
    }

    [Fact]
    public void WithStaticApplicationName_registers_provider()
    {
        var services = new ServiceCollection();
        services.WithStaticApplicationName("test-app");

        using var provider = services.BuildServiceProvider();
        var instance = provider.GetService<IApplicationNameProvider>();

        var typed = Assert.IsType<StaticApplicationNameProvider>(instance);
        Assert.Equal("test-app", typed.Name);
    }
}

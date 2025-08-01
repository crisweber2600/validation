using Microsoft.Extensions.DependencyInjection;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Providers;
using Xunit;

namespace Validation.Tests;

public class EntityIdAndAppNameProviderTests
{
    [Fact]
    public void AddReflectionBasedEntityIdProvider_registers_provider()
    {
        var services = new ServiceCollection();
        services.AddReflectionBasedEntityIdProvider();

        using var provider = services.BuildServiceProvider();
        var idProvider = provider.GetService<IEntityIdProvider>();

        Assert.NotNull(idProvider);
    }

    [Fact]
    public void WithStaticApplicationName_registers_provider()
    {
        const string name = "TestApp";
        var services = new ServiceCollection();
        services.WithStaticApplicationName(name);

        using var provider = services.BuildServiceProvider();
        var nameProvider = provider.GetRequiredService<IApplicationNameProvider>();

        Assert.Equal(name, nameProvider.Name);
    }
}

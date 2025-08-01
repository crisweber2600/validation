using Microsoft.Extensions.DependencyInjection;
using Validation.Infrastructure;
using Validation.Infrastructure.DI;

namespace Validation.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddReflectionBasedEntityIdProvider_registers_provider()
    {
        var services = new ServiceCollection();
        services.AddReflectionBasedEntityIdProvider("Id");

        using var provider = services.BuildServiceProvider();
        var idProvider = provider.GetService<IEntityIdProvider>();

        Assert.NotNull(idProvider);
        Assert.IsType<ReflectionBasedEntityIdProvider>(idProvider);
    }

    [Fact]
    public void WithStaticApplicationName_registers_provider()
    {
        var services = new ServiceCollection();
        services.WithStaticApplicationName("test-app");

        using var provider = services.BuildServiceProvider();
        var nameProvider = provider.GetService<IApplicationNameProvider>();

        Assert.NotNull(nameProvider);
        Assert.IsType<StaticApplicationNameProvider>(nameProvider);
        Assert.Equal("test-app", nameProvider.ApplicationName);
    }
}

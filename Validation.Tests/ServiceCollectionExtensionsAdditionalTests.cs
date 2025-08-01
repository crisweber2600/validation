using System;
using Microsoft.Extensions.DependencyInjection;
using Validation.Infrastructure;
using Validation.Infrastructure.DI;
using Xunit;

public class ServiceCollectionExtensionsAdditionalTests
{
    private class CustomEntity
    {
        public Guid CustomId { get; set; }
    }

    [Fact]
    public void AddReflectionBasedEntityIdProvider_resolves_provider()
    {
        var services = new ServiceCollection();
        services.AddReflectionBasedEntityIdProvider("CustomId");

        using var provider = services.BuildServiceProvider();
        var idProvider = provider.GetRequiredService<IEntityIdProvider>();

        var entity = new CustomEntity { CustomId = Guid.NewGuid() };
        var id = idProvider.GetId(entity);

        Assert.Equal(entity.CustomId, id);
    }

    [Fact]
    public void WithStaticApplicationName_resolves_provider()
    {
        var services = new ServiceCollection();
        services.WithStaticApplicationName("TestApp");

        using var provider = services.BuildServiceProvider();
        var nameProvider = provider.GetRequiredService<IApplicationNameProvider>();

        Assert.Equal("TestApp", nameProvider.GetApplicationName());
    }
}

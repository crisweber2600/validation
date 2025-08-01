using Microsoft.Extensions.DependencyInjection;
using Validation.Infrastructure;
using Validation.Domain;
using Validation.Infrastructure.DI;

namespace Validation.Tests;

public class ReflectionBasedEntityIdProviderTests
{
    private class Sample
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void GetId_returns_priority_property_if_available()
    {
        var provider = new ReflectionBasedEntityIdProvider("Name");
        var sample = new Sample { Name = "Hello" };

        var id = provider.GetId(sample);

        Assert.Equal("Hello", id);
    }

    [Fact]
    public void GetId_falls_back_to_Id_property()
    {
        var provider = new ReflectionBasedEntityIdProvider("Code");
        var sample = new Sample { Id = Guid.NewGuid(), Name = string.Empty };

        var id = provider.GetId(sample);

        Assert.Equal(sample.Id.ToString(), id);
    }

    [Fact]
    public void AddValidationInfrastructure_registers_providers()
    {
        var services = new ServiceCollection();
        services.AddValidationInfrastructure();

        using var provider = services.BuildServiceProvider();
        var idProvider = provider.GetService<IEntityIdProvider>();
        var appNameProvider = provider.GetService<IApplicationNameProvider>();

        Assert.NotNull(idProvider);
        Assert.NotNull(appNameProvider);
        Assert.IsType<ReflectionBasedEntityIdProvider>(idProvider);
        Assert.IsType<StaticApplicationNameProvider>(appNameProvider);
        Assert.Equal("ValidationService", appNameProvider!.GetName());
    }
}

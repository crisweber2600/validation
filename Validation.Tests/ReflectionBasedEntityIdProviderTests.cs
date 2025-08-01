using Microsoft.Extensions.DependencyInjection;
using Validation.Infrastructure;
using Validation.Infrastructure.DI;
using Validation.Domain;

namespace Validation.Tests;

public class ReflectionBasedEntityIdProviderTests
{
    private class NamedEntity { public string Name { get; set; } = "Test"; }
    private class IdEntity { public Guid Id { get; set; } = Guid.NewGuid(); }

    [Fact]
    public void GetId_returns_named_property_when_available()
    {
        var provider = new ReflectionBasedEntityIdProvider();
        var entity = new NamedEntity();
        var id = provider.GetId(entity);
        Assert.Equal("Test", id);
    }

    [Fact]
    public void GetId_falls_back_to_Id_property()
    {
        var entity = new IdEntity();
        var provider = new ReflectionBasedEntityIdProvider();
        var id = provider.GetId(entity);
        Assert.Equal(entity.Id.ToString(), id);
    }

    [Fact]
    public void AddValidationInfrastructure_registers_providers()
    {
        var services = new ServiceCollection();
        services.AddValidationInfrastructure();
        using var sp = services.BuildServiceProvider();
        Assert.NotNull(sp.GetService<IEntityIdProvider>());
        Assert.NotNull(sp.GetService<IApplicationNameProvider>());
    }
}

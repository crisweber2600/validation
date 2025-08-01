using Microsoft.Extensions.DependencyInjection;
using Validation.Infrastructure.DI;
using Validation.Infrastructure;
using Validation.Domain;
using Xunit;

namespace Validation.Tests;

public class AddValidationInfrastructureTests
{
    [Fact]
    public void Providers_Are_Registered()
    {
        var services = new ServiceCollection();
        services.AddValidationInfrastructure();

        using var provider = services.BuildServiceProvider();
        Assert.IsType<ReflectionBasedEntityIdProvider>(provider.GetService<IEntityIdProvider>());
        Assert.IsType<StaticApplicationNameProvider>(provider.GetService<IApplicationNameProvider>());
    }
}

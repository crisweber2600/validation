using Microsoft.Extensions.DependencyInjection;
using Validation.Domain.Entities;
using Validation.Infrastructure.DI;
using Validation.Infrastructure;

namespace Validation.Tests;

public class ManualValidatorServiceTests
{
    [Fact]
    public void Validate_uses_registered_rule()
    {
        var services = new ServiceCollection();
        services.AddValidatorRule<Item>(i => i.Metric > 10);
        var provider = services.BuildServiceProvider();

        var svc = provider.GetRequiredService<IManualValidatorService>();
        Assert.True(svc.Validate(new Item(20)));
        Assert.False(svc.Validate(new Item(5)));
    }
}

using Microsoft.Extensions.DependencyInjection;
using Validation.Domain.Entities;
using Validation.Infrastructure.DI;
using Validation.Infrastructure;

namespace Validation.Tests;

public class ManualValidatorServiceTests
{
    [Fact]
    public void Validate_custom_rules()
    {
        var services = new ServiceCollection();
        services.AddValidatorRule<Item>(i => i.Metric > 0);
        var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<IManualValidatorService>();

        Assert.True(validator.Validate(new Item(1)));
        Assert.False(validator.Validate(new Item(-1)));
    }
}

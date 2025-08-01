using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Validation.Domain.Validation;
using Validation.Infrastructure.DI;

namespace Validation.Tests;

public class ManualValidatorServiceTests
{
    [Fact]
    public void Registered_rule_is_invoked()
    {
        var services = new ServiceCollection();
        services.AddValidatorRule<string>(s => s.Length > 3);
        var provider = services.BuildServiceProvider();
        var svc = provider.GetRequiredService<IManualValidatorService>();
        Assert.True(svc.Validate("hello"));
        Assert.False(svc.Validate("hi"));
    }

    private class YourEntity
    {
        public int Id { get; set; }
    }

    [Fact]
    public void Manual_rules_can_be_added_at_runtime()
    {
        var services = new ServiceCollection();
        services.AddValidatorService()
                .AddValidatorRule<YourEntity>(e => e.Id % 2 == 0);

        var provider = services.BuildServiceProvider();
        var svc = provider.GetRequiredService<IManualValidatorService>();

        Assert.True(svc.Validate(new YourEntity { Id = 4 }));
        Assert.False(svc.Validate(new YourEntity { Id = 3 }));
    }
}
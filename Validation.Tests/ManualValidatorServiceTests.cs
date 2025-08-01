using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Validation.Domain.Validation;
using Validation.Infrastructure.DI;
using Validation.Infrastructure;

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

    [Fact]
    public void Multiple_rules_for_same_type_are_all_enforced()
    {
        var svc = new ManualValidatorService();
        svc.AddRule<string>(s => s.Length > 3);
        svc.AddRule<string>(s => s.StartsWith("h"));

        Assert.True(svc.Validate("hello"));
        Assert.False(svc.Validate("hi"));
        Assert.False(svc.Validate("world"));
    }
}
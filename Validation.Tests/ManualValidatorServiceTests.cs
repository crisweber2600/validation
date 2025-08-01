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
    public void GetRules_returns_added_rules_and_RemoveRules_clears_them()
    {
        var service = new ManualValidatorService();
        service.AddRule<string>(s => s.Length > 1);
        service.AddRule<string>(s => s.StartsWith("A"));

        Assert.Equal(2, service.GetRules(typeof(string)).Count());

        service.RemoveRules(typeof(string));

        Assert.Empty(service.GetRules(typeof(string)));
    }
}
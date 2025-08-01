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
    private class MyEntity
    {
        public int Id { get; set; }
    }

    [Fact]
    public void Runtime_rule_addition_works()
    {
        var services = new ServiceCollection();
        services.AddValidatorService()
                .AddValidatorRule<MyEntity>(e => e.Id % 2 == 0);
        var provider = services.BuildServiceProvider();
        var svc = provider.GetRequiredService<IManualValidatorService>();
        Assert.True(svc.Validate(new MyEntity { Id = 4 }));
        Assert.False(svc.Validate(new MyEntity { Id = 3 }));
    }
}

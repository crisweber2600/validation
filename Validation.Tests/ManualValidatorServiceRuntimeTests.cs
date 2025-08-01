using Microsoft.Extensions.DependencyInjection;
using Validation.Domain.Validation;
using Validation.Infrastructure.DI;
using Xunit;

namespace Validation.Tests;

public class ManualValidatorServiceRuntimeTests
{
    private class TestEntity { public int Id { get; set; } }

    [Fact]
    public void Manual_rule_added_at_runtime_is_invoked()
    {
        var services = new ServiceCollection();
        services.AddValidatorService().AddValidatorRule<TestEntity>(e => e.Id % 2 == 0);
        var provider = services.BuildServiceProvider();
        var svc = provider.GetRequiredService<IManualValidatorService>();
        Assert.True(svc.Validate(new TestEntity { Id = 4 }));
        Assert.False(svc.Validate(new TestEntity { Id = 3 }));
    }
}

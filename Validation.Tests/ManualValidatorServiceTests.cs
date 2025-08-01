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
}
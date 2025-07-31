using Microsoft.Extensions.DependencyInjection;
using Validation.Infrastructure.DI;
using Validation.Infrastructure;

namespace Validation.Tests;

public class ManualValidatorServiceTests
{
    private record Dummy(int Value);

    [Fact]
    public void Validate_runs_registered_predicate()
    {
        var services = new ServiceCollection();
        services.AddValidatorRule<Dummy>(d => d.Value > 10);
        var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<IManualValidatorService>();

        Assert.True(validator.Validate(new Dummy(11)));
        Assert.False(validator.Validate(new Dummy(5)));
    }

    [Fact]
    public void AddValidationFlow_registers_service()
    {
        var services = new ServiceCollection();
        services.AddValidationFlow<AlwaysValidRule>();
        var provider = services.BuildServiceProvider();
        var svc = provider.GetRequiredService<IManualValidatorService>();
        Assert.NotNull(svc);
    }
}

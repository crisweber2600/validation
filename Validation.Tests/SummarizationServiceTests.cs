using Validation.Domain.Validation;
using Validation.Infrastructure.Pipeline;

namespace Validation.Tests;

public class SummarizationServiceTests
{
    [Fact]
    public void Summarize_returns_sum_for_strategy_sum()
    {
        var svc = new SummarizationService(ValidationStrategy.Sum);
        var result = svc.Summarize(new[] {1m, 2m, 3m});
        Assert.Equal(6m, result);
    }

    [Fact]
    public void Summarize_returns_average_for_strategy_average()
    {
        var svc = new SummarizationService(ValidationStrategy.Average);
        var result = svc.Summarize(new[] {2m, 4m});
        Assert.Equal(3m, result);
    }
}


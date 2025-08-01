using System;
using System.Threading.Tasks;
using Validation.Domain.Entities;
using Validation.Tests;
using Xunit;

namespace Validation.Tests;

public class SummaryRecordRepositoryTests
{
    [Fact]
    public async Task AddAndGetLatest_ReturnsLatestRecord()
    {
        var repo = new InMemorySummaryRecordRepository();
        var first = new SummaryRecord { ProgramName = "App", Entity = "Item", MetricValue = 1, RecordedAt = DateTime.UtcNow.AddMinutes(-1), RuntimeId = Guid.NewGuid() };
        var second = new SummaryRecord { ProgramName = "App", Entity = "Item", MetricValue = 2, RecordedAt = DateTime.UtcNow, RuntimeId = Guid.NewGuid() };

        await repo.AddAsync(first);
        await repo.AddAsync(second);

        var latest = await repo.GetLatestAsync("Item");
        Assert.NotNull(latest);
        Assert.Equal(2, latest!.MetricValue);
    }
}

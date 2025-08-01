using System;
using System.Threading.Tasks;
using Validation.Domain.Entities;
using Validation.Tests;
using Xunit;

namespace Validation.Tests;

public class SummaryRecordRepositoryTests
{
    [Fact]
    public async Task GetLatestAsync_ReturnsLatestRecord()
    {
        var repo = new InMemorySummaryRecordRepository();
        var runtimeId = Guid.NewGuid();
        await repo.AddAsync(new SummaryRecord
        {
            ProgramName = "prog",
            Entity = "entity",
            MetricValue = 1,
            RecordedAt = DateTime.UtcNow.AddMinutes(-1),
            RuntimeId = runtimeId
        });
        var latest = new SummaryRecord
        {
            ProgramName = "prog",
            Entity = "entity",
            MetricValue = 2,
            RecordedAt = DateTime.UtcNow,
            RuntimeId = runtimeId
        };
        await repo.AddAsync(latest);

        var result = await repo.GetLatestAsync("prog", "entity");
        Assert.NotNull(result);
        Assert.Equal(2, result!.MetricValue);
    }
}

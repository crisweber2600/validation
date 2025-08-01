using Validation.Domain.Entities;

namespace Validation.Tests;

public class SummaryRecordRepositoryTests
{
    [Fact]
    public async Task AddAndRetrieve_LatestRecord()
    {
        var repo = new InMemorySummaryRecordRepository();
        var record = new SummaryRecord { ProgramName = "app", Entity = "entity", MetricValue = 1, RuntimeId = Guid.NewGuid() };
        await repo.AddAsync(record);

        var latest = await repo.GetLatestAsync("app", "entity");
        Assert.NotNull(latest);
        Assert.Equal(record.MetricValue, latest!.MetricValue);
    }

    [Fact]
    public async Task GetLatestAsync_ReturnsMostRecent()
    {
        var repo = new InMemorySummaryRecordRepository();
        var older = new SummaryRecord { ProgramName = "app", Entity = "entity", MetricValue = 1, RuntimeId = Guid.NewGuid(), RecordedAt = DateTime.UtcNow.AddMinutes(-1) };
        var newer = new SummaryRecord { ProgramName = "app", Entity = "entity", MetricValue = 2, RuntimeId = Guid.NewGuid(), RecordedAt = DateTime.UtcNow };
        await repo.AddAsync(older);
        await repo.AddAsync(newer);

        var latest = await repo.GetLatestAsync("app", "entity");
        Assert.Equal(newer.MetricValue, latest!.MetricValue);
    }
}

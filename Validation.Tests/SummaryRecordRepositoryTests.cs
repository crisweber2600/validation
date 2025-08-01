using Microsoft.EntityFrameworkCore;
using Validation.Domain.Entities;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class SummaryRecordRepositoryTests
{
    [Fact]
    public async Task Add_and_get_latest_returns_most_recent_record()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("summary_records")
            .Options;

        using var context = new TestDbContext(options);
        var repo = new EfCoreSummaryRecordRepository(context);

        var runtimeId = Guid.NewGuid();
        await repo.AddAsync(new SummaryRecord
        {
            ProgramName = "prog",
            Entity = "entity1",
            MetricValue = 1.0m,
            RuntimeId = runtimeId,
            RecordedAt = DateTime.UtcNow.AddMinutes(-1)
        });

        await repo.AddAsync(new SummaryRecord
        {
            ProgramName = "prog",
            Entity = "entity1",
            MetricValue = 2.0m,
            RuntimeId = runtimeId,
            RecordedAt = DateTime.UtcNow
        });

        var latest = await repo.GetLatestAsync("prog", "entity1");

        Assert.NotNull(latest);
        Assert.Equal(2.0m, latest!.MetricValue);
    }
}

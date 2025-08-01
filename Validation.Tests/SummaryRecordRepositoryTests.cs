using Microsoft.EntityFrameworkCore;
using Mongo2Go;
using MongoDB.Driver;
using Validation.Domain.Entities;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class SummaryRecordRepositoryTests
{
    [Fact]
    public async Task EfCore_repository_saves_and_fetches_latest_metric()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("summaryrepo")
            .Options;
        using var context = new TestDbContext(options);
        var repo = new EfCoreSummaryRecordRepository(context);

        var older = new SummaryRecord
        {
            ProgramName = "prog",
            Entity = "ent",
            MetricValue = 1m,
            RecordedAt = DateTime.UtcNow.AddMinutes(-5),
            RuntimeId = Guid.NewGuid()
        };
        var newer = new SummaryRecord
        {
            ProgramName = "prog",
            Entity = "ent",
            MetricValue = 5m,
            RecordedAt = DateTime.UtcNow,
            RuntimeId = Guid.NewGuid()
        };

        await repo.AddAsync(older);
        await repo.AddAsync(newer);

        var latest = await repo.GetLatestValueAsync("prog", "ent");
        Assert.Equal(5m, latest);
    }

    [Fact(Skip = "MongoDB server not available in CI environment")]
    public async Task Mongo_repository_saves_and_fetches_latest_metric()
    {
        using var runner = MongoDbRunner.Start();
        var client = new MongoClient(runner.ConnectionString);
        var database = client.GetDatabase("testdb");
        var repo = new MongoSummaryRecordRepository(database);

        var older = new SummaryRecord
        {
            ProgramName = "prog",
            Entity = "ent",
            MetricValue = 2m,
            RecordedAt = DateTime.UtcNow.AddMinutes(-5),
            RuntimeId = Guid.NewGuid()
        };
        var newer = new SummaryRecord
        {
            ProgramName = "prog",
            Entity = "ent",
            MetricValue = 8m,
            RecordedAt = DateTime.UtcNow,
            RuntimeId = Guid.NewGuid()
        };

        await repo.AddAsync(older);
        await repo.AddAsync(newer);

        var latest = await repo.GetLatestValueAsync("prog", "ent");
        Assert.Equal(8m, latest);
    }
}

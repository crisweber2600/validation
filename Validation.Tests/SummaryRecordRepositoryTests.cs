using Microsoft.EntityFrameworkCore;
using Mongo2Go;
using MongoDB.Driver;
using Validation.Domain.Entities;
using Validation.Domain.Repositories;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class SummaryRecordRepositoryTests
{
    [Fact]
    public async Task EfCoreRepository_AddAndGetLatest_Works()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("summaryef")
            .Options;
        await using var context = new TestDbContext(options);
        var repo = new EfCoreSummaryRecordRepository(context);
        var older = new SummaryRecord { ProgramName = "prog", Entity = "ent", MetricValue = 1, RecordedAt = DateTime.UtcNow.AddMinutes(-1), RuntimeId = Guid.NewGuid() };
        var newer = new SummaryRecord { ProgramName = "prog", Entity = "ent", MetricValue = 2, RecordedAt = DateTime.UtcNow, RuntimeId = Guid.NewGuid() };
        await repo.AddAsync(older);
        await repo.AddAsync(newer);

        var latest = await repo.GetLatestValueAsync("prog", "ent");
        Assert.Equal(2m, latest);
    }

    [Fact(Skip = "MongoDB not available in test environment")]
    public async Task MongoRepository_AddAndGetLatest_Works()
    {
        using var runner = MongoDbRunner.Start();
        var client = new MongoClient(runner.ConnectionString);
        var database = client.GetDatabase("testdb");
        var repo = new MongoSummaryRecordRepository(database);
        var older = new SummaryRecord { ProgramName = "prog", Entity = "ent", MetricValue = 3, RecordedAt = DateTime.UtcNow.AddMinutes(-1), RuntimeId = Guid.NewGuid() };
        var newer = new SummaryRecord { ProgramName = "prog", Entity = "ent", MetricValue = 4, RecordedAt = DateTime.UtcNow, RuntimeId = Guid.NewGuid() };
        await repo.AddAsync(older);
        await repo.AddAsync(newer);

        var latest = await repo.GetLatestValueAsync("prog", "ent");
        Assert.Equal(4m, latest);
    }
}

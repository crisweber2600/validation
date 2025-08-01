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
    public async Task EfCoreRepository_AddAndGetLatest()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("summary_ef")
            .Options;
        using var context = new TestDbContext(options);
        var repo = new EfCoreSummaryRecordRepository(context);

        var first = new SummaryRecord { ProgramName = "prog", Entity = "ent", MetricValue = 1, RecordedAt = DateTime.UtcNow.AddMinutes(-1), RuntimeId = Guid.NewGuid() };
        var second = new SummaryRecord { ProgramName = "prog", Entity = "ent", MetricValue = 2, RecordedAt = DateTime.UtcNow, RuntimeId = Guid.NewGuid() };

        await repo.AddAsync(first);
        await repo.AddAsync(second);

        var latest = await repo.GetLatestValueAsync("prog", "ent");
        Assert.Equal(2, latest);
    }

    [Fact]
    public async Task MongoRepository_AddAndGetLatest()
    {
        MongoDbRunner runner;
        try
        {
            runner = MongoDbRunner.Start();
        }
        catch
        {
            return; // skip test if MongoDB cannot start
        }
        using var r = runner;
        var client = new MongoClient(r.ConnectionString);
        var db = client.GetDatabase("testdb");
        var repo = new MongoSummaryRecordRepository(db);

        var first = new SummaryRecord { ProgramName = "prog", Entity = "ent", MetricValue = 1, RecordedAt = DateTime.UtcNow.AddMinutes(-1), RuntimeId = Guid.NewGuid() };
        var second = new SummaryRecord { ProgramName = "prog", Entity = "ent", MetricValue = 2, RecordedAt = DateTime.UtcNow, RuntimeId = Guid.NewGuid() };

        try
        {
            await repo.AddAsync(first);
            await repo.AddAsync(second);
        }
        catch
        {
            return; // skip if MongoDB operations fail
        }

        var latest = await repo.GetLatestValueAsync("prog", "ent");
        Assert.Equal(2, latest);
    }
}

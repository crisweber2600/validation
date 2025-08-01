using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Validation.Domain.Validation;
using Validation.Infrastructure;
using Validation.Domain;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class UnitOfWorkExampleTests
{
    private class YourEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public decimal Metric { get; set; }
    }

    private class ExampleDbContext : DbContext
    {
        public ExampleDbContext(DbContextOptions<ExampleDbContext> options) : base(options) { }
        public DbSet<SaveAudit> SaveAudits => Set<SaveAudit>();
        public DbSet<NannyRecord> NannyRecords => Set<NannyRecord>();
        public DbSet<YourEntity> Entities => Set<YourEntity>();
    }

    [Fact]
    public async Task UnitOfWork_usage_example()
    {
        var services = new ServiceCollection();
        services.AddDbContext<ExampleDbContext>(o => o.UseInMemoryDatabase("uowexample"));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ExampleDbContext>());
        services.AddSingleton<IValidationPlanProvider, InMemoryValidationPlanProvider>();
        services.AddScoped<SummarisationValidator>();
        services.AddScoped<INannyRecordRepository, InMemoryNannyRecordRepository>();
        services.AddScoped<UnitOfWork>();

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var planProvider = scope.ServiceProvider.GetRequiredService<IValidationPlanProvider>();
        planProvider.AddPlan<YourEntity>(new ValidationPlan(e => ((YourEntity)e).Metric, ThresholdType.RawDifference, 5));

        var uow = scope.ServiceProvider.GetRequiredService<UnitOfWork>();
        await uow.Repository<YourEntity>().AddAsync(new YourEntity { Metric = 50 });
        var count = await uow.SaveChangesWithPlanAsync<YourEntity>();

        var ctx = scope.ServiceProvider.GetRequiredService<ExampleDbContext>();
        var repo = scope.ServiceProvider.GetRequiredService<INannyRecordRepository>() as InMemoryNannyRecordRepository;
        Assert.NotNull(repo);
        Assert.Equal(1, ctx.Entities.Count());
        Assert.Equal(1, count);
        Assert.Single(repo!.Records);
        Assert.Equal(50m, repo.Records[0].LastMetric);
    }
}
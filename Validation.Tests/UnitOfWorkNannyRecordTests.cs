using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure;
using Validation.Infrastructure.Repositories;
using Validation.Domain;

namespace Validation.Tests;

public class UnitOfWorkNannyRecordTests
{
    private class ExampleDbContext : DbContext
    {
        public ExampleDbContext(DbContextOptions<ExampleDbContext> options) : base(options) { }
        public DbSet<SaveAudit> SaveAudits => Set<SaveAudit>();
        public DbSet<NannyRecord> NannyRecords => Set<NannyRecord>();
        public DbSet<Item> Items => Set<Item>();
    }

    [Fact]
    public async Task SaveChanges_updates_nanny_record()
    {
        var services = new ServiceCollection();
        services.AddDbContext<ExampleDbContext>(o => o.UseInMemoryDatabase("nanny"));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ExampleDbContext>());
        services.AddSingleton<IValidationPlanProvider, InMemoryValidationPlanProvider>();
        services.AddScoped<SummarisationValidator>();
        services.AddScoped<INannyRecordRepository, InMemoryNannyRecordRepository>();
        services.AddScoped<UnitOfWork>();

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var planProvider = scope.ServiceProvider.GetRequiredService<IValidationPlanProvider>();
        planProvider.AddPlan<Item>(new ValidationPlan(e => ((Item)e).Metric, ThresholdType.RawDifference, 5));

        var uow = scope.ServiceProvider.GetRequiredService<UnitOfWork>();
        var item = new Item(10m);
        await uow.Repository<Item>().AddAsync(item);

        await uow.SaveChangesWithPlanAsync<Item>();

        var nannyRepo = scope.ServiceProvider.GetRequiredService<INannyRecordRepository>() as InMemoryNannyRecordRepository;
        Assert.NotNull(nannyRepo);
        Assert.Single(nannyRepo!.Records);
        var record = nannyRepo!.Records.Single();
        Assert.Equal(item.Id, record.EntityId);
        Assert.Equal(10m, record.LastMetric);
    }
}

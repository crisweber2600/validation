using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class UnitOfWorkExampleTests
{
    [Fact]
    public async Task UnitOfWork_usage_example()
    {
        var services = new ServiceCollection();
        services.AddDbContext<YourDbContext>(o => o.UseInMemoryDatabase("uowtest"));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<YourDbContext>());
        services.AddSingleton<IValidationPlanProvider, InMemoryValidationPlanProvider>();
        services.AddScoped<SummarisationValidator>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        var provider = services.BuildServiceProvider();
        var planProvider = provider.GetRequiredService<IValidationPlanProvider>();
        planProvider.AddPlan<YourEntity>(
            new ValidationPlan(e => ((YourEntity)e).Id, ThresholdType.RawDifference, 5));

        using var scope = provider.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var repo = uow.Repository<YourEntity>();
        await repo.AddAsync(new YourEntity { Id = 50 });
        await uow.SaveChangesWithPlanAsync<YourEntity>();

        var context = scope.ServiceProvider.GetRequiredService<YourDbContext>();
        var count = context.YourEntities.Count();
        Assert.Equal(1, count);
    }
}

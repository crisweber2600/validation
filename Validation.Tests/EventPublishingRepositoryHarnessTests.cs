using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Validation.Domain.Entities;
using Validation.Domain.Events;
using Validation.Domain.Repositories;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class EventPublishingRepositoryHarnessTests
{
    [Fact]
    public async Task SaveAsync_publishes_SaveRequested_with_matching_entity()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness();
        services.AddScoped<IEntityRepository<Item>, EventPublishingRepository<Item>>();

        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();
        try
        {
            using var scope = provider.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IEntityRepository<Item>>();
            var entity = new Item(10);
            await repo.SaveAsync(entity);

            var published = await harness.Published.SelectAsync<SaveRequested<Item>>().FirstOrDefault();
            Assert.NotNull(published);
            Assert.Equal(entity.Id, published.Context.Message.Entity.Id);
            Assert.Equal(entity.Metric, published.Context.Message.Entity.Metric);
        }
        finally
        {
            await harness.Stop();
        }
    }
}

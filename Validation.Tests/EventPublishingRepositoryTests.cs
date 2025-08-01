using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Validation.Domain.Entities;
using Validation.Domain.Events;
using Validation.Domain.Repositories;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class EventPublishingRepositoryTests
{
    [Fact]
    public async Task SaveAsync_publishes_event()
    {
        var harness = new InMemoryTestHarness();

        await harness.Start();
        try
        {
            var repository = new EventPublishingRepository<Item>(harness.Bus);
            var item = new Item(5);
            await repository.SaveAsync(item);
            Assert.True(await harness.Published.Any<SaveRequested<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task DeleteAsync_publishes_event()
    {
        var harness = new InMemoryTestHarness();

        await harness.Start();
        try
        {
            var repository = new EventPublishingRepository<Item>(harness.Bus);
            var id = Guid.NewGuid();
            await repository.DeleteAsync(id);
            Assert.True(await harness.Published.Any<DeleteRequested>());
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task SaveAsync_publishes_event_with_payload()
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
            var repository = scope.ServiceProvider.GetRequiredService<IEntityRepository<Item>>();
            var item = new Item(5);
            await repository.SaveAsync(item);

            Assert.True(await harness.Published.Any<SaveRequested<Item>>(x =>
                x.Context.Message.Entity.Id == item.Id &&
                x.Context.Message.Entity.Metric == item.Metric));
        }
        finally
        {
            await harness.Stop();
        }
    }
}
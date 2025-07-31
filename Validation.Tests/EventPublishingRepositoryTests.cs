using MassTransit.Testing;
using Validation.Domain.Entities;
using Validation.Domain.Events;
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
}
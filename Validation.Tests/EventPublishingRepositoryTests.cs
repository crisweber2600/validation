using MassTransit.Testing;
using Validation.Domain.Entities;
using ValidationFlow.Messages;
using Validation.Infrastructure.Repositories;
using System.Linq;

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
            var repository = new EventPublishingRepository<Item>(harness.Bus, "MyApp");
            var item = new Item(5);
            await repository.SaveAsync(item);
            var msg = harness.Published.Select<SaveRequested<Item>>().First().Context.Message;
            Assert.Equal("MyApp", msg.AppName);
            Assert.Equal(nameof(Item), msg.EntityType);
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
            var repository = new EventPublishingRepository<Item>(harness.Bus, "MyApp");
            var id = Guid.NewGuid();
            await repository.DeleteAsync(id);
            var msg = harness.Published.Select<DeleteRequested<Item>>().First().Context.Message;
            Assert.Equal("MyApp", msg.AppName);
            Assert.Equal(nameof(Item), msg.EntityType);
        }
        finally
        {
            await harness.Stop();
        }
    }
}
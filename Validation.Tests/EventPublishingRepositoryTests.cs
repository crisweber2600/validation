using MassTransit.Testing;
using System.Linq;
using Validation.Domain.Entities;
using ValidationFlow.Messages;
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
            var repository = new EventPublishingRepository<Item>(harness.Bus, "TestApp");
            var item = new Item(5);
            await repository.SaveAsync(item);
            Assert.True(await harness.Published.Any<SaveRequested<Item>>());
            var published = harness.Published.Select<SaveRequested<Item>>().First().Context.Message;
            Assert.Equal("TestApp", published.AppName);
            Assert.Equal("Item", published.EntityType);
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
            var repository = new EventPublishingRepository<Item>(harness.Bus, "TestApp");
            var id = Guid.NewGuid();
            await repository.DeleteAsync(id);
            Assert.True(await harness.Published.Any<DeleteRequested<Item>>());
            var published = harness.Published.Select<DeleteRequested<Item>>().First().Context.Message;
            Assert.Equal("TestApp", published.AppName);
            Assert.Equal("Item", published.EntityType);
        }
        finally
        {
            await harness.Stop();
        }
    }
}
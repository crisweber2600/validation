using MassTransit.Testing;
using MassTransit;
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
            var repository = new EventPublishingRepository<Item>(harness.Bus);
            var item = new Item(5);
            await repository.SaveAsync(item, "TestApp");
            Assert.True(await harness.Published.Any<SaveRequested<Item>>());
            var message = (await harness.Published
                .SelectAsync<SaveRequested<Item>>().First()).Context.Message;
            Assert.Equal("TestApp", message.AppName);
            Assert.Equal("Item", message.EntityType);
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
            await repository.DeleteAsync(id, "TestApp");
            Assert.True(await harness.Published.Any<DeleteRequested<Item>>());
            var message = (await harness.Published
                .SelectAsync<DeleteRequested<Item>>().First()).Context.Message;
            Assert.Equal(id, message.EntityId);
            Assert.Equal("TestApp", message.AppName);
            Assert.Equal("Item", message.EntityType);
        }
        finally
        {
            await harness.Stop();
        }
    }
}
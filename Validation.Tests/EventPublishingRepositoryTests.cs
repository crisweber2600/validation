using Xunit;
using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Entities;
using Validation.Domain.Events;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class EventPublishingRepositoryTests
{
    [Fact]
    public async Task SaveAsync_publishes_SaveRequested_event()
    {
        var harness = new InMemoryTestHarness();
        await harness.Start();
        try
        {
            var repository = new EventPublishingRepository<Item>(harness.Bus);
            var item = new Item(10);

            await repository.SaveAsync(item);

            Assert.True(await harness.Published.Any<SaveRequested<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}

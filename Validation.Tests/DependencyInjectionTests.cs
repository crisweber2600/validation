using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Messaging;

namespace Validation.Tests;

public class DependencyInjectionTests
{
    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) {}
        public DbSet<SaveAudit> SaveAudits => Set<SaveAudit>();
    }

    private class SaveValidatedHandler : IConsumer<SaveValidated>
    {
        public TaskCompletionSource<SaveValidated> Source { get; } = new();
        public Task Consume(ConsumeContext<SaveValidated> context)
        {
            Source.TrySetResult(context.Message);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Validation_flow_wires_up_dependencies_and_emits_event()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IValidationRule>(new RawDifferenceRule(100));
        services.AddValidationFlow<RawDifferenceRule>(builder =>
        {
            builder.SetupDatabase<TestDbContext>("testdb");
        });

        await using var provider = services.BuildServiceProvider(true);

        await using var scope = provider.CreateAsyncScope();
        var consumer = scope.ServiceProvider.GetRequiredService<SaveRequestedConsumer>();
        var handler = new SaveValidatedHandler();

        var harness = new InMemoryTestHarness();
        var consumerHarness = harness.Consumer(() => consumer);
        harness.Consumer(() => handler);

        await harness.Start();
        try
        {
            await harness.Bus.Publish(new SaveRequested(Guid.NewGuid()));

            Assert.True(await harness.Consumed.Any<SaveRequested>());
            Assert.True(await consumerHarness.Consumed.Any<SaveRequested>());
            await handler.Source.Task.WaitAsync(TimeSpan.FromSeconds(3));

            var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            Assert.Equal(1, await ctx.Set<SaveAudit>().CountAsync());
        }
        finally
        {
            await harness.Stop();
        }
    }
}

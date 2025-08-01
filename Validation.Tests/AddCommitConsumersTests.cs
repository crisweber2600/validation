using Microsoft.Extensions.DependencyInjection;
using Validation.Domain.Entities;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Messaging;
using Xunit;

namespace Validation.Tests;

public class AddCommitConsumersTests
{
    [Fact]
    public void AddSaveCommit_registers_consumer()
    {
        var services = new ServiceCollection();
        services.AddSaveCommit<Item>();

        using var provider = services.BuildServiceProvider();
        var consumer = provider.GetService<SaveCommitConsumer<Item>>();
        Assert.NotNull(consumer);
    }

    [Fact]
    public void AddDeleteCommit_registers_consumer()
    {
        var services = new ServiceCollection();
        services.AddDeleteCommit<Item>();

        using var provider = services.BuildServiceProvider();
        var consumer = provider.GetService<DeleteCommitConsumer<Item>>();
        Assert.NotNull(consumer);
    }
}

using Microsoft.Extensions.DependencyInjection;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Messaging;
using Validation.Infrastructure.Repositories;
using Validation.Domain.Entities;

namespace Validation.Tests;

public class AddCommitExtensionsTests
{
    [Fact]
    public void AddSaveCommit_registers_consumer()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISaveAuditRepository, InMemorySaveAuditRepository>();
        services.AddSaveCommit<Item>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var consumer = scope.ServiceProvider.GetService<SaveCommitConsumer<Item>>();
        Assert.NotNull(consumer);
    }

    [Fact]
    public void AddDeleteCommit_registers_consumer()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISaveAuditRepository, InMemorySaveAuditRepository>();
        services.AddDeleteCommit<Item>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var consumer = scope.ServiceProvider.GetService<DeleteCommitConsumer<Item>>();
        Assert.NotNull(consumer);
    }
}

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
        services.AddScoped<ISaveAuditRepository, InMemorySaveAuditRepository>();
        services.AddSaveCommit<Item>();

        Assert.Contains(services, d => d.ServiceType == typeof(SaveCommitConsumer<Item>));
    }

    [Fact]
    public void AddDeleteCommit_registers_consumer()
    {
        var services = new ServiceCollection();
        services.AddScoped<ISaveAuditRepository, InMemorySaveAuditRepository>();
        services.AddDeleteCommit<Item>();

        Assert.Contains(services, d => d.ServiceType == typeof(DeleteCommitConsumer<Item>));
    }
}

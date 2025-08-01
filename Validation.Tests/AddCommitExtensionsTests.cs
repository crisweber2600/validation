using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Messaging;
using Validation.Domain.Entities;

namespace Validation.Tests;

public class AddCommitExtensionsTests
{
    [Fact]
    public void AddSaveCommit_registers_consumer()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(o => o.UseInMemoryDatabase("save-commit"));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TestDbContext>());
        services.AddSaveCommit<Item>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.NotNull(scope.ServiceProvider.GetService<SaveCommitConsumer<Item>>());
    }

    [Fact]
    public void AddDeleteCommit_registers_consumer()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(o => o.UseInMemoryDatabase("delete-commit"));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TestDbContext>());
        services.AddDeleteCommit<Item>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.NotNull(scope.ServiceProvider.GetService<DeleteCommitConsumer<Item>>());
    }
}

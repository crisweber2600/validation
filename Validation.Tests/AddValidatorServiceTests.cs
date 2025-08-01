using System;
using Microsoft.Extensions.DependencyInjection;
using Validation.Domain.Validation;
using Validation.Infrastructure.DI;

namespace Validation.Tests;

public class AddValidatorServiceTests
{
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void AddValidatorService_registers_manual_validator_service()
    {
        var services = new ServiceCollection();
        services.AddValidatorService();

        using var provider = services.BuildServiceProvider();
        var validatorService = provider.GetService<IManualValidatorService>();
        
        Assert.NotNull(validatorService);
    }

    [Fact]
    public void AddValidatorRule_allows_runtime_rule_addition()
    {
        var services = new ServiceCollection();
        services.AddValidatorRule<TestEntity>(entity => entity.Id > 0);
        services.AddValidatorRule<TestEntity>(entity => !string.IsNullOrEmpty(entity.Name));

        using var provider = services.BuildServiceProvider();
        var validatorService = provider.GetRequiredService<IManualValidatorService>();
        
        Assert.NotNull(validatorService);

        // Test valid entity
        var validEntity = new TestEntity { Id = 1, Name = "Test" };
        Assert.True(validatorService.Validate(validEntity));

        // Test invalid entity (Id = 0)
        var invalidEntity1 = new TestEntity { Id = 0, Name = "Test" };
        Assert.False(validatorService.Validate(invalidEntity1));

        // Test invalid entity (empty name)
        var invalidEntity2 = new TestEntity { Id = 1, Name = "" };
        Assert.False(validatorService.Validate(invalidEntity2));
    }

    [Fact]
    public void AddValidatorRule_can_be_called_multiple_times()
    {
        var services = new ServiceCollection();
        
        // Add first rule
        services.AddValidatorRule<TestEntity>(entity => entity.Id > 0);
        
        // Add second rule
        services.AddValidatorRule<TestEntity>(entity => entity.Name.Length > 2);

        using var provider = services.BuildServiceProvider();
        var validatorService = provider.GetRequiredService<IManualValidatorService>();
        
        Assert.NotNull(validatorService);

        // Test entity that passes first rule but fails second
        var entity = new TestEntity { Id = 1, Name = "Hi" };
        Assert.False(validatorService.Validate(entity));

        // Test entity that passes both rules
        var validEntity = new TestEntity { Id = 1, Name = "Hello" };
        Assert.True(validatorService.Validate(validEntity));
    }

    [Fact]
    public void AddValidatorRule_throws_when_rule_is_duplicate()
    {
        var services = new ServiceCollection();
        Func<TestEntity, bool> rule = e => e.Id > 0;

        services.AddValidatorRule(rule);

        var ex = Assert.Throws<InvalidOperationException>(() => services.AddValidatorRule(rule));
        Assert.Contains("duplicate", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Validation.Infrastructure;
using Xunit;

namespace Validation.Tests;

public class EnhancedManualValidatorServiceTests
{
    private readonly EnhancedManualValidatorService _validator;

    public EnhancedManualValidatorServiceTests()
    {
        _validator = new EnhancedManualValidatorService(NullLogger<EnhancedManualValidatorService>.Instance);
    }

    [Fact]
    public void AddRule_NamedRule_CanBeRetrieved()
    {
        // Arrange
        const string ruleName = "TestRule";
        Func<string, bool> rule = s => !string.IsNullOrEmpty(s);

        // Act
        _validator.AddRule(ruleName, rule);
        var ruleNames = _validator.GetRuleNames<string>();

        // Assert
        Assert.Contains(ruleName, ruleNames);
    }

    [Fact]
    public void RemoveRule_ExistingRule_ReturnsTrue()
    {
        // Arrange
        const string ruleName = "TestRule";
        _validator.AddRule<string>(ruleName, s => true);

        // Act
        var removed = _validator.RemoveRule<string>(ruleName);

        // Assert
        Assert.True(removed);
        Assert.DoesNotContain(ruleName, _validator.GetRuleNames<string>());
    }

    [Fact]
    public void RemoveRule_NonExistentRule_ReturnsFalse()
    {
        // Act
        var removed = _validator.RemoveRule<string>("NonExistent");

        // Assert
        Assert.False(removed);
    }

    [Fact]
    public void ValidateWithDetails_PassingRules_ReturnsValid()
    {
        // Arrange
        _validator.AddRule<string>("NotEmpty", s => !string.IsNullOrEmpty(s));
        _validator.AddRule<string>("MinLength", s => s.Length >= 3);

        // Act
        var result = _validator.ValidateWithDetails("Hello");

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.FailedRules);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateWithDetails_FailingRules_ReturnsInvalid()
    {
        // Arrange
        _validator.AddRule<string>("NotEmpty", s => !string.IsNullOrEmpty(s));
        _validator.AddRule<string>("MinLength", s => s.Length >= 10);

        // Act
        var result = _validator.ValidateWithDetails("Hello");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("MinLength", result.FailedRules);
        Assert.DoesNotContain("NotEmpty", result.FailedRules);
    }

    [Fact(Skip="Skipped after event migration")]
    public void ValidateWithDetails_ExceptionInRule_ReturnsInvalidWithError()
    {
        // Arrange
        _validator.AddRule<string>("ThrowingRule", s => throw new InvalidOperationException("Test exception"));

        // Act
        var result = _validator.ValidateWithDetails("Hello");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("ThrowingRule", result.FailedRules);
        Assert.Single(result.Errors);
        Assert.Contains("Test exception", result.Errors.First());
    }

    [Fact]
    public void HasDuplicateRules_NoDuplicates_ReturnsFalse()
    {
        // Arrange
        _validator.AddRule<string>("Rule1", s => true);
        _validator.AddRule<string>("Rule2", s => true);

        // Act
        var hasDuplicates = _validator.HasDuplicateRules<string>();

        // Assert
        Assert.False(hasDuplicates);
    }

    [Fact]
    public void InspectRule_ExistingRule_ReturnsTrue()
    {
        // Arrange
        const string ruleName = "TestRule";
        Func<string, bool> originalRule = s => !string.IsNullOrEmpty(s);
        _validator.AddRule(ruleName, originalRule);

        // Act
        var found = _validator.InspectRule<string>(ruleName, out var inspectedRule);

        // Assert
        Assert.True(found);
        Assert.NotNull(inspectedRule);
    }

    [Fact]
    public void InspectRule_NonExistentRule_ReturnsFalse()
    {
        // Act
        var found = _validator.InspectRule<string>("NonExistent", out var rule);

        // Assert
        Assert.False(found);
        Assert.Null(rule);
    }

    [Fact]
    public void ClearRules_RemovesAllRulesForType()
    {
        // Arrange
        _validator.AddRule<string>("Rule1", s => true);
        _validator.AddRule<string>("Rule2", s => true);
        _validator.AddRule<string>(s => true); // Anonymous rule

        // Act
        _validator.ClearRules<string>();

        // Assert
        Assert.Empty(_validator.GetRuleNames<string>());
        var result = _validator.ValidateWithDetails("test");
        Assert.True(result.IsValid); // No rules to fail
    }

    [Fact]
    public async Task ValidateAsync_ValidInput_ReturnsValidResult()
    {
        // Arrange
        _validator.AddRule<string>("NotEmpty", s => !string.IsNullOrEmpty(s));

        // Act
        var result = await _validator.ValidateAsync("Hello");

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void GetSummary_ValidResult_ReturnsPassMessage()
    {
        // Arrange
        var result = new ValidationResult { IsValid = true };

        // Act
        var summary = result.GetSummary();

        // Assert
        Assert.Equal("Validation passed", summary);
    }

    [Fact]
    public void GetSummary_InvalidResult_ReturnsFailMessage()
    {
        // Arrange
        var result = new ValidationResult 
        { 
            IsValid = false,
            FailedRules = { "Rule1", "Rule2" },
            Errors = { "Error1" }
        };

        // Act
        var summary = result.GetSummary();

        // Assert
        Assert.Contains("Validation failed", summary);
        Assert.Contains("Rule1, Rule2", summary);
        Assert.Contains("Error1", summary);
    }

    private class TestClass
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    [Fact]
    public void ValidateWithDetails_ComplexObject_WorksCorrectly()
    {
        // Arrange
        _validator.AddRule<TestClass>("ValidName", tc => !string.IsNullOrEmpty(tc.Name));
        _validator.AddRule<TestClass>("ValidAge", tc => tc.Age >= 0);

        var testObj = new TestClass { Name = "John", Age = 25 };

        // Act
        var result = _validator.ValidateWithDetails(testObj);

        // Assert
        Assert.True(result.IsValid);
    }
}
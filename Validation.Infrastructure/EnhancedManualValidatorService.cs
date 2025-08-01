using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Validation.Domain.Validation;

namespace Validation.Infrastructure;

public interface IEnhancedManualValidatorService : IManualValidatorService
{
    void AddRule<T>(string ruleName, Func<T, bool> rule);
    bool RemoveRule<T>(string ruleName);
    IEnumerable<string> GetRuleNames<T>();
    IEnumerable<string> GetRuleNames(Type type);
    bool HasDuplicateRules<T>();
    ValidationResult ValidateWithDetails(object instance);
    Task<ValidationResult> ValidateAsync<T>(T instance);
    void ClearRules<T>();
    void ClearAllRules();
    bool InspectRule<T>(string ruleName, out Func<T, bool>? rule);
}

public class EnhancedManualValidatorService : IEnhancedManualValidatorService
{
    private readonly ConcurrentDictionary<Type, Dictionary<string, Func<object, bool>>> _namedRules = new();
    private readonly ConcurrentDictionary<Type, List<Func<object, bool>>> _anonymousRules = new();
    private readonly ILogger<EnhancedManualValidatorService> _logger;

    public EnhancedManualValidatorService(ILogger<EnhancedManualValidatorService> logger)
    {
        _logger = logger;
    }

    public void AddRule<T>(Func<T, bool> rule)
    {
        if (rule == null) throw new ArgumentNullException(nameof(rule));

        var list = _anonymousRules.GetOrAdd(typeof(T), _ => new List<Func<object, bool>>());
        lock (list)
        {
            list.Add(o => rule((T)o));
        }

        _logger.LogDebug("Added anonymous rule for type {Type}. Total anonymous rules: {Count}",
            typeof(T).Name, list.Count);
    }

    public void AddRule<T>(string ruleName, Func<T, bool> rule)
    {
        if (string.IsNullOrWhiteSpace(ruleName)) throw new ArgumentNullException(nameof(ruleName));
        if (rule == null) throw new ArgumentNullException(nameof(rule));

        var rules = _namedRules.GetOrAdd(typeof(T), _ => new Dictionary<string, Func<object, bool>>());
        lock (rules)
        {
            if (rules.ContainsKey(ruleName))
            {
                _logger.LogWarning("Rule {RuleName} for type {Type} already exists. Replacing.", ruleName, typeof(T).Name);
            }

            rules[ruleName] = o => rule((T)o);
        }

        _logger.LogDebug("Added named rule {RuleName} for type {Type}. Total named rules: {Count}",
            ruleName, typeof(T).Name, rules.Count);
    }

    public bool RemoveRule<T>(string ruleName)
    {
        if (string.IsNullOrWhiteSpace(ruleName)) return false;

        if (_namedRules.TryGetValue(typeof(T), out var rules))
        {
            lock (rules)
            {
                var removed = rules.Remove(ruleName);
                if (removed)
                {
                    _logger.LogDebug("Removed rule {RuleName} for type {Type}", ruleName, typeof(T).Name);
                }
                return removed;
            }
        }

        return false;
    }

    public IEnumerable<string> GetRuleNames<T>()
    {
        return GetRuleNames(typeof(T));
    }

    public IEnumerable<string> GetRuleNames(Type type)
    {
        if (_namedRules.TryGetValue(type, out var rules))
        {
            lock (rules)
            {
                return rules.Keys.ToList();
            }
        }

        return Enumerable.Empty<string>();
    }

    public bool HasDuplicateRules<T>()
    {
        var ruleNames = GetRuleNames<T>().ToList();
        return ruleNames.Count != ruleNames.Distinct().Count();
    }

    public bool Validate(object instance)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));

        var result = ValidateWithDetails(instance);
        return result.IsValid;
    }

    public ValidationResult ValidateWithDetails(object instance)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));

        var type = instance.GetType();
        var result = new ValidationResult { IsValid = true };

        try
        {
            // Validate with named rules
            if (_namedRules.TryGetValue(type, out var namedRules))
            {
                lock (namedRules)
                {
                    foreach (var kvp in namedRules)
                    {
                        try
                        {
                            var isValid = kvp.Value(instance);
                            if (!isValid)
                            {
                                result.IsValid = false;
                                result.FailedRules.Add(kvp.Key);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error executing named rule {RuleName} for type {Type}",
                                kvp.Key, type.Name);
                            result.FailedRules.Add(kvp.Key);
                            result.IsValid = false;
                            result.Errors.Add($"Rule '{kvp.Key}' execution failed: {ex.Message}");
                        }
                    }
                }
            }

            // Validate with anonymous rules
            if (_anonymousRules.TryGetValue(type, out var anonymousRules))
            {
                lock (anonymousRules)
                {
                    for (int i = 0; i < anonymousRules.Count; i++)
                    {
                        try
                        {
                            var isValid = anonymousRules[i](instance);
                            if (!isValid)
                            {
                                result.IsValid = false;
                                result.FailedRules.Add($"Anonymous rule {i}");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error executing anonymous rule {Index} for type {Type}",
                                i, type.Name);
                            result.IsValid = false;
                            result.Errors.Add($"Anonymous rule {i} execution failed: {ex.Message}");
                        }
                    }
                }
            }

            _logger.LogDebug("Validation completed for type {Type}. Result: {IsValid}. Failed rules: {FailedCount}",
                type.Name, result.IsValid, result.FailedRules.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during validation for type {Type}", type.Name);
            result.IsValid = false;
            result.Errors.Add($"Validation framework error: {ex.Message}");
        }

        return result;
    }

    public async Task<ValidationResult> ValidateAsync<T>(T instance)
    {
        return await Task.Run(() => ValidateWithDetails(instance!));
    }

    public void ClearRules<T>()
    {
        var type = typeof(T);

        if (_namedRules.TryGetValue(type, out var namedRules))
        {
            lock (namedRules)
            {
                namedRules.Clear();
            }
        }

        if (_anonymousRules.TryGetValue(type, out var anonymousRules))
        {
            lock (anonymousRules)
            {
                anonymousRules.Clear();
            }
        }

        _logger.LogDebug("Cleared all rules for type {Type}", typeof(T).Name);
    }

    public void ClearAllRules()
    {
        _namedRules.Clear();
        _anonymousRules.Clear();
        _logger.LogInformation("Cleared all validation rules");
    }

    public bool InspectRule<T>(string ruleName, out Func<T, bool>? rule)
    {
        rule = null;

        if (string.IsNullOrWhiteSpace(ruleName)) return false;

        if (_namedRules.TryGetValue(typeof(T), out var rules))
        {
            lock (rules)
            {
                if (rules.TryGetValue(ruleName, out var objectRule))
                {
                    rule = o => objectRule(o!);
                    return true;
                }
            }
        }

        return false;
    }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> FailedRules { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    
    public string GetSummary()
    {
        if (IsValid) return "Validation passed";
        
        var summary = $"Validation failed. Failed rules: {string.Join(", ", FailedRules)}";
        if (Errors.Any())
        {
            summary += $". Errors: {string.Join(", ", Errors)}";
        }
        return summary;
    }
}
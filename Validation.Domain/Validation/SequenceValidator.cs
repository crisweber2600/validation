using System;
using System.Collections.Generic;
using System.Linq;

namespace Validation.Domain.Validation;

/// <summary>
/// Interface for sequence validation operations
/// </summary>
public interface ISequenceValidator
{
    /// <summary>
    /// Validate that a sequence is in ascending order
    /// </summary>
    bool IsAscending<T>(IEnumerable<T> sequence) where T : IComparable<T>;
    
    /// <summary>
    /// Validate that a sequence is in descending order
    /// </summary>
    bool IsDescending<T>(IEnumerable<T> sequence) where T : IComparable<T>;
    
    /// <summary>
    /// Validate that a sequence has no duplicates
    /// </summary>
    bool HasNoDuplicates<T>(IEnumerable<T> sequence);
    
    /// <summary>
    /// Validate that a sequence is contiguous (no gaps)
    /// </summary>
    bool IsContiguous<T>(IEnumerable<T> sequence, Func<T, T, bool> areConsecutive);
    
    /// <summary>
    /// Get detailed validation result for a sequence
    /// </summary>
    SequenceValidationResult ValidateSequence<T>(IEnumerable<T> sequence, SequenceValidationOptions<T> options);
}

/// <summary>
/// Options for sequence validation
/// </summary>
public class SequenceValidationOptions<T>
{
    public bool RequireAscending { get; set; }
    public bool RequireDescending { get; set; }
    public bool AllowDuplicates { get; set; } = true;
    public bool RequireContiguous { get; set; }
    public Func<T, T, bool>? ContiguityChecker { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public Func<T, bool>? ItemValidator { get; set; }
}

/// <summary>
/// Result of sequence validation
/// </summary>
public class SequenceValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public int ItemCount { get; set; }
    public bool IsAscending { get; set; }
    public bool IsDescending { get; set; }
    public bool HasDuplicates { get; set; }
    public bool IsContiguous { get; set; }
    public List<int> DuplicateIndices { get; set; } = new();
    public List<int> OrderViolationIndices { get; set; } = new();
    public List<int> ContiguityGapIndices { get; set; } = new();
}

/// <summary>
/// Implementation of sequence validator with comprehensive validation capabilities
/// </summary>
public class SequenceValidator : ISequenceValidator
{
    public bool IsAscending<T>(IEnumerable<T> sequence) where T : IComparable<T>
    {
        if (sequence == null) return false;
        
        var list = sequence.ToList();
        if (list.Count <= 1) return true;
        
        for (int i = 1; i < list.Count; i++)
        {
            if (list[i - 1].CompareTo(list[i]) > 0)
                return false;
        }
        
        return true;
    }

    public bool IsDescending<T>(IEnumerable<T> sequence) where T : IComparable<T>
    {
        if (sequence == null) return false;
        
        var list = sequence.ToList();
        if (list.Count <= 1) return true;
        
        for (int i = 1; i < list.Count; i++)
        {
            if (list[i - 1].CompareTo(list[i]) < 0)
                return false;
        }
        
        return true;
    }

    public bool HasNoDuplicates<T>(IEnumerable<T> sequence)
    {
        if (sequence == null) return false;
        
        var set = new HashSet<T>();
        foreach (var item in sequence)
        {
            if (!set.Add(item))
                return false;
        }
        
        return true;
    }

    public bool IsContiguous<T>(IEnumerable<T> sequence, Func<T, T, bool> areConsecutive)
    {
        if (sequence == null || areConsecutive == null) return false;
        
        var list = sequence.ToList();
        if (list.Count <= 1) return true;
        
        for (int i = 1; i < list.Count; i++)
        {
            if (!areConsecutive(list[i - 1], list[i]))
                return false;
        }
        
        return true;
    }

    public SequenceValidationResult ValidateSequence<T>(IEnumerable<T> sequence, SequenceValidationOptions<T> options)
    {
        var result = new SequenceValidationResult();
        
        if (sequence == null)
        {
            result.Errors.Add("Sequence cannot be null");
            return result;
        }
        
        var list = sequence.ToList();
        result.ItemCount = list.Count;
        
        // Validate length constraints
        if (options.MinLength.HasValue && list.Count < options.MinLength.Value)
        {
            result.Errors.Add($"Sequence length {list.Count} is below minimum {options.MinLength.Value}");
        }
        
        if (options.MaxLength.HasValue && list.Count > options.MaxLength.Value)
        {
            result.Errors.Add($"Sequence length {list.Count} exceeds maximum {options.MaxLength.Value}");
        }
        
        // Validate individual items
        if (options.ItemValidator != null)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (!options.ItemValidator(list[i]))
                {
                    result.Errors.Add($"Item at index {i} failed validation");
                }
            }
        }
        
        // Check for duplicates
        var duplicates = list
            .Select((item, index) => new { item, index })
            .GroupBy(x => x.item)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g.Select(x => x.index))
            .ToList();
            
        result.HasDuplicates = duplicates.Any();
        result.DuplicateIndices = duplicates;
        
        if (!options.AllowDuplicates && result.HasDuplicates)
        {
            result.Errors.Add($"Duplicates found at indices: {string.Join(", ", duplicates)}");
        }
        
        // Check ordering if needed
        if (list.Count > 1 && typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
        {
            // Use dynamic casting for IComparable<T> types
            var isAscending = true;
            var isDescending = true;
            
            for (int i = 1; i < list.Count; i++)
            {
                var comparison = Comparer<T>.Default.Compare(list[i - 1], list[i]);
                if (comparison > 0) isAscending = false;
                if (comparison < 0) isDescending = false;
            }
            
            result.IsAscending = isAscending;
            result.IsDescending = isDescending;
            
            if (options.RequireAscending && !result.IsAscending)
            {
                result.Errors.Add("Sequence is not in ascending order");
                // Find violations
                for (int i = 1; i < list.Count; i++)
                {
                    if (Comparer<T>.Default.Compare(list[i - 1], list[i]) > 0)
                        result.OrderViolationIndices.Add(i);
                }
            }
            
            if (options.RequireDescending && !result.IsDescending)
            {
                result.Errors.Add("Sequence is not in descending order");
                // Find violations
                for (int i = 1; i < list.Count; i++)
                {
                    if (Comparer<T>.Default.Compare(list[i - 1], list[i]) < 0)
                        result.OrderViolationIndices.Add(i);
                }
            }
        }
        
        // Check contiguity if needed
        if (options.RequireContiguous && options.ContiguityChecker != null)
        {
            result.IsContiguous = IsContiguous(list, options.ContiguityChecker);
            
            if (!result.IsContiguous)
            {
                result.Errors.Add("Sequence is not contiguous");
                // Find gaps
                for (int i = 1; i < list.Count; i++)
                {
                    if (!options.ContiguityChecker(list[i - 1], list[i]))
                        result.ContiguityGapIndices.Add(i);
                }
            }
        }
        
        result.IsValid = !result.Errors.Any();
        return result;
    }
}

/// <summary>
/// Utility methods for common sequence validation patterns
/// </summary>
public static class SequenceValidationUtilities
{
    /// <summary>
    /// Check if integers are consecutive
    /// </summary>
    public static bool AreConsecutiveIntegers(int first, int second) => second == first + 1;
    
    /// <summary>
    /// Check if dates are consecutive days
    /// </summary>
    public static bool AreConsecutiveDays(DateTime first, DateTime second) => second.Date == first.Date.AddDays(1);
    
    /// <summary>
    /// Check if dates are consecutive hours
    /// </summary>
    public static bool AreConsecutiveHours(DateTime first, DateTime second) => second == first.AddHours(1);
    
    /// <summary>
    /// Validate a numeric sequence with common patterns
    /// </summary>
    public static SequenceValidationResult ValidateNumericSequence<T>(IEnumerable<T> sequence, bool requireAscending = false, bool allowDuplicates = true) 
        where T : IComparable<T>
    {
        var validator = new SequenceValidator();
        var options = new SequenceValidationOptions<T>
        {
            RequireAscending = requireAscending,
            AllowDuplicates = allowDuplicates
        };
        
        return validator.ValidateSequence(sequence, options);
    }
    
    /// <summary>
    /// Validate a date sequence with common patterns
    /// </summary>
    public static SequenceValidationResult ValidateDateSequence(IEnumerable<DateTime> sequence, bool requireContiguous = false, bool requireAscending = true)
    {
        var validator = new SequenceValidator();
        var options = new SequenceValidationOptions<DateTime>
        {
            RequireAscending = requireAscending,
            RequireContiguous = requireContiguous,
            ContiguityChecker = requireContiguous ? AreConsecutiveDays : null,
            AllowDuplicates = false
        };
        
        return validator.ValidateSequence(sequence, options);
    }
}
using System;
using System.Threading.Tasks;

namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Base interface for pipeline orchestrators
/// </summary>
public interface IPipelineOrchestrator<T>
{
    Task<PipelineResult> ExecuteAsync(T input);
    Task<PipelineResult> ExecuteWithWorkerAsync(T input, WorkerConfig config);
}

/// <summary>
/// Configuration for pipeline workers
/// </summary>
public class WorkerConfig
{
    public int MaxConcurrency { get; set; } = 1;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
    public int MaxRetries { get; set; } = 3;
}

/// <summary>
/// Result of pipeline execution
/// </summary>
public class PipelineResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration { get; set; }
    public object? Data { get; set; }
    
    public static PipelineResult Successful(object? data = null, TimeSpan duration = default)
        => new() { Success = true, Data = data, Duration = duration };
    
    public static PipelineResult Failed(string errorMessage, TimeSpan duration = default)
        => new() { Success = false, ErrorMessage = errorMessage, Duration = duration };
}
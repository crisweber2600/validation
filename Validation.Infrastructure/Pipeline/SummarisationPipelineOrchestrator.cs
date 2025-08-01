using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Pipeline orchestrator for summarisation workflows
/// </summary>
public class SummarisationPipelineOrchestrator : IPipelineOrchestrator<SummarisationInput>
{
    private readonly ILogger<SummarisationPipelineOrchestrator> _logger;

    public SummarisationPipelineOrchestrator(ILogger<SummarisationPipelineOrchestrator> logger)
    {
        _logger = logger;
    }

    public async Task<PipelineResult> ExecuteAsync(SummarisationInput input)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation(
                "Starting summarisation pipeline for {Count} records of type {EntityType}",
                input.Records?.Count ?? 0,
                input.EntityType);

            if (input.Records == null || !input.Records.Any())
            {
                return PipelineResult.Failed("No records provided for summarisation");
            }

            // Basic summarisation logic
            var summary = new SummarisationResult
            {
                EntityType = input.EntityType,
                TotalRecords = input.Records.Count,
                ProcessedAt = DateTime.UtcNow,
                Aggregates = CalculateAggregates(input.Records),
                Summary = GenerateSummary(input.Records)
            };

            stopwatch.Stop();
            _logger.LogInformation(
                "Summarisation pipeline completed for {EntityType} in {Duration}ms. Processed {Count} records",
                input.EntityType,
                stopwatch.ElapsedMilliseconds,
                input.Records.Count);

            return PipelineResult.Successful(summary, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Summarisation pipeline failed for {EntityType}", input.EntityType);
            return PipelineResult.Failed(ex.Message, stopwatch.Elapsed);
        }
    }

    public async Task<PipelineResult> ExecuteWithWorkerAsync(SummarisationInput input, WorkerConfig config)
    {
        if (input.Records == null || input.Records.Count <= config.MaxConcurrency)
        {
            return await ExecuteAsync(input);
        }

        // Process in batches for large datasets
        var batches = input.Records
            .Select((record, index) => new { record, index })
            .GroupBy(x => x.index / config.MaxConcurrency)
            .Select(g => g.Select(x => x.record).ToList())
            .ToList();

        var tasks = batches.Select(batch => ExecuteAsync(new SummarisationInput
        {
            EntityType = input.EntityType,
            Records = batch,
            GroupBy = input.GroupBy,
            Filters = input.Filters
        }));

        var results = await Task.WhenAll(tasks);
        
        // Combine results
        var successfulResults = results.Where(r => r.Success).ToList();
        var failedResults = results.Where(r => !r.Success).ToList();

        if (failedResults.Any())
        {
            var combinedDuration = results.Aggregate(TimeSpan.Zero, (acc, r) => acc + r.Duration);
            return PipelineResult.Failed(
                $"Failed to process {failedResults.Count} batches: {string.Join(", ", failedResults.Select(r => r.ErrorMessage))}",
                combinedDuration);
        }

        // Aggregate all successful results
        var combinedSummary = CombineSummaries(successfulResults.Select(r => r.Data).Cast<SummarisationResult>());
        var totalDuration = results.Aggregate(TimeSpan.Zero, (acc, r) => acc + r.Duration);

        return PipelineResult.Successful(combinedSummary, totalDuration);
    }

    private Dictionary<string, object> CalculateAggregates(List<object> records)
    {
        return new Dictionary<string, object>
        {
            ["TotalCount"] = records.Count,
            ["ProcessedAt"] = DateTime.UtcNow
        };
    }

    private string GenerateSummary(List<object> records)
    {
        return $"Processed {records.Count} records at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";
    }

    private SummarisationResult CombineSummaries(IEnumerable<SummarisationResult> summaries)
    {
        var summaryList = summaries.ToList();
        if (!summaryList.Any())
            return new SummarisationResult { EntityType = "Unknown", TotalRecords = 0 };

        var first = summaryList.First();
        return new SummarisationResult
        {
            EntityType = first.EntityType,
            TotalRecords = summaryList.Sum(s => s.TotalRecords),
            ProcessedAt = DateTime.UtcNow,
            Aggregates = summaryList
                .SelectMany(s => s.Aggregates)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.Last().Value),
            Summary = $"Combined {summaryList.Count} summaries with {summaryList.Sum(s => s.TotalRecords)} total records"
        };
    }
}

/// <summary>
/// Input data for summarisation pipeline
/// </summary>
public class SummarisationInput
{
    public string EntityType { get; set; } = string.Empty;
    public List<object>? Records { get; set; }
    public string? GroupBy { get; set; }
    public Dictionary<string, object> Filters { get; set; } = new();
}

/// <summary>
/// Result of summarisation processing
/// </summary>
public class SummarisationResult
{
    public string EntityType { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public DateTime ProcessedAt { get; set; }
    public Dictionary<string, object> Aggregates { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}
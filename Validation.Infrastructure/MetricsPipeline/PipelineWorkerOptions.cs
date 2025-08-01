using System;

namespace Validation.Infrastructure;

public class PipelineWorkerOptions
{
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(5);
}

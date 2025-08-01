namespace Validation.Infrastructure.Pipeline;

public class InMemoryGatherService : IGatherService
{
    private readonly IEnumerable<double> _values;
    public InMemoryGatherService(IEnumerable<double> values)
    {
        _values = values;
    }

    public Task<IEnumerable<double>> GatherAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_values);
}

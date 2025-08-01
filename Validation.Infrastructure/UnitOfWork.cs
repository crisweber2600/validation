using Microsoft.EntityFrameworkCore;
using Validation.Infrastructure.Repositories;
using Validation.Domain.Validation;
using Validation.Domain;

namespace Validation.Infrastructure;

public class UnitOfWork
{
    private readonly DbContext _context;
    private readonly IValidationPlanProvider _planProvider;
    private readonly SummarisationValidator _validator;
    private readonly INannyRecordRepository _nannyRepository;
    private readonly string _programName;
    private readonly string _runtimeIdentifier;
    private readonly Dictionary<Type, object> _repos = new();

    public UnitOfWork(DbContext context, IValidationPlanProvider planProvider, SummarisationValidator validator, INannyRecordRepository nannyRepository)
    {
        _context = context;
        _planProvider = planProvider;
        _validator = validator;
        _nannyRepository = nannyRepository;
        _programName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "unknown";
        _runtimeIdentifier = System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier;
    }

    public IGenericRepository<T> Repository<T>() where T : class
    {
        if (!_repos.TryGetValue(typeof(T), out var repo))
        {
            repo = new EfGenericRepository<T>(_context, _planProvider, _validator);
            _repos[typeof(T)] = repo;
        }
        return (IGenericRepository<T>)repo;
    }

    public async Task<int> SaveChangesWithPlanAsync<T>(CancellationToken ct = default) where T : class
    {
        var entries = _context.ChangeTracker.Entries<T>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();

        await Repository<T>().SaveChangesWithPlanAsync(ct);

        var plan = _planProvider.GetPlan(typeof(T));
        foreach (var entry in entries)
        {
            var idProp = typeof(T).GetProperty("Id");
            if (idProp == null) continue;
            if (!(idProp.GetValue(entry.Entity) is Guid entityId)) continue;

            decimal metric = 0m;
            if (plan.MetricSelector != null)
            {
                metric = plan.MetricSelector(entry.Entity);
            }

            var last = await _nannyRepository.GetLastAsync(entityId, ct);
            if (last != null)
            {
                last.LastMetric = metric;
                last.ProgramName = _programName;
                last.RuntimeIdentifier = _runtimeIdentifier;
                last.Timestamp = DateTime.UtcNow;
                await _nannyRepository.UpdateAsync(last, ct);
            }
            else
            {
                await _nannyRepository.AddAsync(new NannyRecord
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    LastMetric = metric,
                    ProgramName = _programName,
                    RuntimeIdentifier = _runtimeIdentifier
                }, ct);
            }
        }

        return await _context.Set<T>().CountAsync(ct);
    }
}
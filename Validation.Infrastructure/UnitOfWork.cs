using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Runtime.InteropServices;
using Validation.Infrastructure.Repositories;
using Validation.Domain.Validation;

namespace Validation.Infrastructure;

public class UnitOfWork
{
    private readonly DbContext _context;
    private readonly IValidationPlanProvider _planProvider;
    private readonly SummarisationValidator _validator;
    private readonly INannyRecordRepository _nannyRepository;
    private readonly Dictionary<Type, object> _repos = new();

    public UnitOfWork(DbContext context, IValidationPlanProvider planProvider, SummarisationValidator validator, INannyRecordRepository nannyRepository)
    {
        _context = context;
        _planProvider = planProvider;
        _validator = validator;
        _nannyRepository = nannyRepository;
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
        await Repository<T>().SaveChangesWithPlanAsync(ct);

        var plan = _planProvider.GetPlan(typeof(T));
        if (plan.MetricSelector != null)
        {
            foreach (var entry in _context.ChangeTracker.Entries<T>().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                var metric = plan.MetricSelector(entry.Entity);
                var idProp = entry.Entity!.GetType().GetProperty("Id");
                if (idProp == null || idProp.PropertyType != typeof(Guid))
                    continue;
                var entityId = (Guid)idProp.GetValue(entry.Entity)!;

                var record = await _nannyRepository.GetLastAsync(entityId, ct);
                if (record == null)
                {
                    record = new Validation.Domain.NannyRecord
                    {
                        Id = Guid.NewGuid(),
                        EntityId = entityId,
                        LastMetric = metric,
                        ProgramName = AppDomain.CurrentDomain.FriendlyName,
                        RuntimeIdentifier = System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier
                    };
                    await _nannyRepository.AddAsync(record, ct);
                }
                else
                {
                    record.LastMetric = metric;
                    record.ProgramName = AppDomain.CurrentDomain.FriendlyName;
                    record.RuntimeIdentifier = System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier;
                    record.Timestamp = DateTime.UtcNow;
                    await _nannyRepository.UpdateAsync(record, ct);
                }
            }
        }

        return await _context.Set<T>().CountAsync(ct);
    }
}
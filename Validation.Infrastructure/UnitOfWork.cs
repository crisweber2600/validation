using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Validation.Domain;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure;

public class UnitOfWork
{
    private readonly DbContext _context;
    private readonly IValidationPlanProvider _planProvider;
    private readonly SummarisationValidator _validator;
    private readonly INannyRecordRepository _nannyRepo;
    private readonly Dictionary<Type, object> _repos = new();

    public UnitOfWork(DbContext context, IValidationPlanProvider planProvider, SummarisationValidator validator, INannyRecordRepository nannyRepo)
    {
        _context = context;
        _planProvider = planProvider;
        _validator = validator;
        _nannyRepo = nannyRepo;
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
            var entries = await _context.Set<T>().ToListAsync(ct);

            foreach (var entity in entries)
            {
                var idProp = entity!.GetType().GetProperty("Id");
                if (idProp == null || idProp.PropertyType != typeof(Guid))
                    continue;

                var entityId = (Guid)(idProp.GetValue(entity) ?? Guid.Empty);
                var metric = plan.MetricSelector(entity);

                var existing = await _nannyRepo.GetByEntityIdAsync(entityId, ct);
                var record = existing ?? new NannyRecord { Id = Guid.NewGuid(), EntityId = entityId };

                record.LastMetric = metric;
                record.ProgramName = AppDomain.CurrentDomain.FriendlyName;
                record.RuntimeIdentifier = System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier;
                record.Timestamp = DateTime.UtcNow;

                if (existing == null)
                    await _nannyRepo.AddAsync(record, ct);
                else
                    await _nannyRepo.UpdateAsync(record, ct);
            }
        }

        return await _context.Set<T>().CountAsync(ct);
    }
}
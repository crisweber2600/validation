using System.Threading;
using System.Threading.Tasks;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.UnitOfWork;

public interface IUnitOfWork
{
    IRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<int> SaveChangesWithPlanAsync<T>(CancellationToken ct = default);
}

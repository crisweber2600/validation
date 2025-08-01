using System;

namespace Validation.Infrastructure;

public interface IEntityIdProvider
{
    Guid GetId<T>(T entity);
}

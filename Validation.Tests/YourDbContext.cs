using Microsoft.EntityFrameworkCore;
using Validation.Infrastructure;
using Validation.Domain.Entities;

namespace Validation.Tests;

public class YourDbContext : DbContext
{
    public YourDbContext(DbContextOptions<YourDbContext> options) : base(options)
    {
    }

    public DbSet<SaveAudit> SaveAudits => Set<SaveAudit>();
    public DbSet<YourEntity> YourEntities => Set<YourEntity>();
}

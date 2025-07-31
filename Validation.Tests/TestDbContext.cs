using Microsoft.EntityFrameworkCore;
using Validation.Infrastructure;

namespace Validation.Tests;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<SaveAudit> SaveAudits => Set<SaveAudit>();
}
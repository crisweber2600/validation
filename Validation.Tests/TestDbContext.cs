using Microsoft.EntityFrameworkCore;
using Validation.Infrastructure;

namespace Validation.Tests;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<SaveAudit> SaveAudits => Set<SaveAudit>();
    public DbSet<Validation.Domain.Entities.Item> Items => Set<Validation.Domain.Entities.Item>();
    public DbSet<Validation.Domain.Entities.SummaryRecord> SummaryRecords => Set<Validation.Domain.Entities.SummaryRecord>();
}
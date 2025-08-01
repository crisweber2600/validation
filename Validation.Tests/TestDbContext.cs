using Microsoft.EntityFrameworkCore;
using Validation.Infrastructure;
using Validation.Domain;

namespace Validation.Tests;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<SaveAudit> SaveAudits => Set<SaveAudit>();
    public DbSet<NannyRecord> NannyRecords => Set<NannyRecord>();
    public DbSet<Validation.Domain.Entities.Item> Items => Set<Validation.Domain.Entities.Item>();
}
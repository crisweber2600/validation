using Microsoft.EntityFrameworkCore;
using Validation.Infrastructure;
using Validation.Domain.Entities;

namespace Validation.Tests;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<SaveAudit> SaveAudits => Set<SaveAudit>();
    public DbSet<Item> Items => Set<Item>();
}
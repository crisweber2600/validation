<Query Kind="Program">
  <NuGetReference>Microsoft.EntityFrameworkCore.InMemory</NuGetReference>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>Microsoft.Extensions.DependencyInjection</Namespace>
</Query>

void Main()
{
    var services = new ServiceCollection();
    services.AddDbContext<YourDbContext>(o => o.UseInMemoryDatabase("demo"));
    services.AddSingleton<ValidationService>();
    using var provider = services.BuildServiceProvider();
    using var scope = provider.CreateScope();
    var ctx = scope.ServiceProvider.GetRequiredService<YourDbContext>();
    ctx.Database.EnsureCreated();
    ctx.Entities.AddRange(Enumerable.Range(1,10).Select(i => new YourEntity{Id=i}));
    ctx.SaveChanges();
    var svc = scope.ServiceProvider.GetRequiredService<ValidationService>();
    var sum = svc.Sum(ctx.Entities);
    var avg = svc.Average(ctx.Entities);
    var count = svc.Count(ctx.Entities);
    var variance = svc.Variance(ctx.Entities);
    Console.WriteLine(sum);
    Console.WriteLine(avg);
    Console.WriteLine(count);
    Console.WriteLine(variance);
}

class YourEntity
{
    public int Id { get; set; }
}

class YourDbContext : DbContext
{
    public YourDbContext(DbContextOptions<YourDbContext> options) : base(options) {}
    public DbSet<YourEntity> Entities => Set<YourEntity>();
}

class ValidationService
{
    public double Sum(IEnumerable<YourEntity> e) => e.Sum(x => (double)x.Id);
    public double Average(IEnumerable<YourEntity> e) => e.Average(x => (double)x.Id);
    public int Count(IEnumerable<YourEntity> e) => e.Count();
    public double Variance(IEnumerable<YourEntity> e)
    {
        var vals = e.Select(x => (double)x.Id).ToArray();
        var avg = vals.Average();
        return vals.Select(v => Math.Pow(v - avg, 2)).Average();
    }
}

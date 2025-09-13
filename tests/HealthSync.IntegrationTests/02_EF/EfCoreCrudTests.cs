using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using HealthSync.IntegrationTests.Common;

namespace HealthSync.IntegrationTests.EF;

public class EfCoreCrudTests
{
    private class SmokeEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    private class SmokeDbContext : DbContext
    {
        private readonly string _cs;
        public SmokeDbContext(string cs) => _cs = cs;
        public DbSet<SmokeEntity> Smoke => Set<SmokeEntity>();
        protected override void OnConfiguring(DbContextOptionsBuilder o) => o.UseSqlServer(_cs);
        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<SmokeEntity>(e =>
            {
                e.ToTable("EfSmoke");
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            });
        }
    }

    [Fact]
    public async Task EnsureCreated_Insert_Select_Should_Work()
    {
        var db = TestSql.NewDb();
        await TestSql.CreateDbAsync(db);
        var cs = TestSql.Cs(db);

        try
        {
            await using (var ctx = new SmokeDbContext(cs))
            {
                await ctx.Database.EnsureCreatedAsync();
                ctx.Smoke.Add(new SmokeEntity { Id = 1, Name = "OK" });
                await ctx.SaveChangesAsync();
            }

            await using (var ctx = new SmokeDbContext(cs))
            {
                (await ctx.Smoke.CountAsync()).Should().Be(1);
            }
        }
        finally
        {
            await TestSql.DropDbAsync(db);
        }
    }
}

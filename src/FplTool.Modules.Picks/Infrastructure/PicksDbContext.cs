using FplTool.Modules.Picks.Domain;
using Microsoft.EntityFrameworkCore;

namespace FplTool.Modules.Picks.Infrastructure;

public sealed class PicksDbContext : DbContext
{
    public PicksDbContext(DbContextOptions<PicksDbContext> options) : base(options) { }

    public DbSet<CaptainPick> CaptainPicks => Set<CaptainPick>();
    public DbSet<GameweekPointsSync> GameweekPointsSyncs => Set<GameweekPointsSync>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CaptainPick>(entity =>
        {
            entity.ToTable("picks_captain_picks");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedNever();
            entity.Property(p => p.PlayerWebName).IsRequired().HasMaxLength(100);
            entity.HasIndex(p => new { p.UserId, p.GameweekId }).IsUnique();
            entity.HasIndex(p => p.GameweekId);
        });

        modelBuilder.Entity<GameweekPointsSync>(entity =>
        {
            entity.ToTable("picks_gw_points_sync");
            entity.HasKey(p => p.GameweekId);
            entity.Property(p => p.GameweekId).ValueGeneratedNever();
        });
    }
}

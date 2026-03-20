using FplTool.Modules.Auth.Domain;
using Microsoft.EntityFrameworkCore;

namespace FplTool.Modules.Auth.Infrastructure;

public sealed class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("auth_users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).ValueGeneratedNever();
            entity.Property(u => u.GoogleSubjectId).IsRequired().HasMaxLength(128);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.Property(u => u.DisplayName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.FplManagerId).IsRequired(false);
            entity.Property(u => u.IsAdmin).IsRequired().HasDefaultValue(false);
            entity.HasIndex(u => u.GoogleSubjectId).IsUnique();
            entity.HasIndex(u => u.FplManagerId);
        });
    }
}

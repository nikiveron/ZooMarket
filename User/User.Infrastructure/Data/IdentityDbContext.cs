using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Identity.Infrastructure.Data;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserClaim> UserClaims => Set<UserClaim>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Role).IsRequired().HasMaxLength(50);

            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<UserClaim>(entity =>
        {
            entity.HasKey(uc => uc.Id);
            entity.HasOne(uc => uc.User)
                  .WithMany(u => u.Claims)
                  .HasForeignKey(uc => uc.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

using Microsoft.EntityFrameworkCore;
using ReviewEntity = Review.Domain.Entities.ReviewEntity;
using ProductReviewsSummaryEntity = Review.Domain.Entities.ProductReviewsSummary;

namespace Review.Infrastructure.Data;

public class ReviewDbContext : DbContext
{
    public ReviewDbContext(DbContextOptions<ReviewDbContext> options) : base(options) { }

    public DbSet<ReviewEntity> Reviews => Set<ReviewEntity>();
    public DbSet<ProductReviewsSummaryEntity> ProductReviewsSummaries => Set<ProductReviewsSummaryEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ReviewEntity>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Rating).IsRequired();
            entity.Property(r => r.Title).HasMaxLength(200);
            entity.Property(r => r.Comment).HasMaxLength(2000);
            entity.HasIndex(r => new { r.ProductId, r.UserId });
        });

        modelBuilder.Entity<ProductReviewsSummaryEntity>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.HasIndex(s => s.ProductId).IsUnique();
        });
    }
}


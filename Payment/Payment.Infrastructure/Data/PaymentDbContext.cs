using Microsoft.EntityFrameworkCore;
using PaymentEntity = Payment.Domain.Entities.PaymentEntity;

namespace Payment.Infrastructure.Data;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<PaymentEntity> Payments => Set<PaymentEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PaymentEntity>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Currency).HasMaxLength(10);
            entity.Property(p => p.PaymentGateway).HasMaxLength(50);
            entity.Property(p => p.GatewayTransactionId).HasMaxLength(200);
        });
    }
}


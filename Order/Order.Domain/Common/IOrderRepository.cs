using Order.Domain.Entities;

namespace Order.Domain.Common;

public interface IOrderRepository
{
    Task<OrderEntity?> GetByIdAsync(Guid id);
    Task<IEnumerable<OrderEntity>> GetOrdersByUserIdAsync(Guid userId);
    Task AddAsync(OrderEntity order);
    Task UpdateAsync(OrderEntity order);
    Task DeleteAsync(OrderEntity order);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    // Для распределенных транзакций
    Task<IDbTransaction> BeginTransactionAsync();
}

public interface IDbTransaction : IAsyncDisposable
{
    Task CommitAsync();
    Task RollbackAsync();
}
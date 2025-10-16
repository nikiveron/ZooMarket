
namespace Order.Domain.Entities;

public class Order : BaseEntity
{
    public Guid UserId { get; set; } // Внешний ключ на пользователя (из Identity Service)
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; } // Внешний ключ на продукт (из Catalog Service)
    public string ProductName { get; set; } = string.Empty; // Денормализованные данные
    public decimal UnitPrice { get; set; } // Денормализованные данные
    public int Quantity { get; set; }
    public Order Order { get; set; } = null!;
}

public class Cart : BaseEntity
{
    public Guid UserId { get; set; }
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public Cart Cart { get; set; } = null!;
}

public class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Paid,
    Shipped,
    Delivered,
    Cancelled
}

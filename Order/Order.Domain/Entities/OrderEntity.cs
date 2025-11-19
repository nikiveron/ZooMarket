using Order.Domain.Common;
using Order.Domain.Events;

namespace Order.Domain.Entities;

public class OrderEntity : BaseEntity
{
    // Приватный конструктор для Entity Framework
    private OrderEntity() { }

    // Публичные свойства
    public Guid UserId { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; private set; }
    public string ShippingAddress { get; private set; } = string.Empty;
    public string BillingAddress { get; private set; } = string.Empty;

    // Навигационные свойства
    public ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();

    public static OrderEntity Create(
    Guid userId,
    string shippingAddress,
    string billingAddress,
    List<OrderItemData> orderItemsData)
    {
        // Валидация входных параметров
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (string.IsNullOrWhiteSpace(shippingAddress))
            throw new ArgumentException("Shipping address is required", nameof(shippingAddress));

        if (string.IsNullOrWhiteSpace(billingAddress))
            throw new ArgumentException("Billing address is required", nameof(billingAddress));

        if (orderItemsData == null || !orderItemsData.Any())
            throw new ArgumentException("Order must contain at least one item", nameof(orderItemsData));

        // Создание заказа
        var order = new OrderEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrderNumber = GenerateOrderNumber(),
            Status = OrderStatus.Pending,
            ShippingAddress = shippingAddress.Trim(),
            BillingAddress = billingAddress.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        // Добавление товаров в заказ
        foreach (var itemData in orderItemsData)
        {
            if (itemData.Quantity <= 0)
                throw new ArgumentException($"Quantity must be greater than 0 for product {itemData.ProductId}");

            // В реальном приложении здесь нужно получить информацию о товаре из Catalog service
            var orderItem = OrderItem.Create(
                itemData.ProductId,
                $"Product {itemData.ProductId}", // Заглушка - в реальном приложении из Catalog
                10.0m, // Заглушка - в реальном приложении из Catalog
                itemData.Quantity);

            order.OrderItems.Add(orderItem);
        }

        // Расчет общей суммы
        order.CalculateTotalAmount();

        // Публикация доменного события
        order.AddDomainEvent(new OrderCreatedEvent(order.Id, order.UserId, order.TotalAmount));

        return order;
    }

    // Метод для обновления статуса заказа
    public void UpdateStatus(OrderStatus newStatus)
    {
        if (Status == newStatus) return;

        // Бизнес-правила для смены статусов
        if (Status == OrderStatus.Cancelled && newStatus != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot change status of cancelled order");

        if (Status == OrderStatus.Delivered && newStatus != OrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot change status of delivered order");

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        // Публикация доменного события при определенных статусах
        if (newStatus == OrderStatus.Confirmed)
        {
            AddDomainEvent(new OrderConfirmedEvent(Id));
        }
        else if (newStatus == OrderStatus.Cancelled)
        {
            AddDomainEvent(new OrderCancelledEvent(Id));
        }
    }

    // Метод для расчета общей суммы
    private void CalculateTotalAmount()
    {
        TotalAmount = OrderItems.Sum(item => item.UnitPrice * item.Quantity);
    }

    // Генерация номера заказа
    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
    }

    // Дополнительные бизнес-методы
    public bool CanBeCancelled()
    {
        return Status == OrderStatus.Pending || Status == OrderStatus.Confirmed;
    }

    public void Cancel(string reason = "")
    {
        if (!CanBeCancelled())
            throw new InvalidOperationException($"Order cannot be cancelled in current status: {Status}");

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderCancelledEvent(Id, reason));
    }
}

public class OrderItem : BaseEntity
{
    // Приватный конструктор
    private OrderItem() { }

    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    // Навигационное свойство
    public OrderEntity Order { get; private set; } = null!;

    // Фабричный метод для OrderItem
    public static OrderItem Create(
        Guid productId,
        string productName,
        decimal unitPrice,
        int quantity)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name is required", nameof(productName));

        if (unitPrice <= 0)
            throw new ArgumentException("Unit price must be greater than 0", nameof(unitPrice));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        return new OrderItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            ProductName = productName.Trim(),
            UnitPrice = unitPrice,
            Quantity = quantity,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Метод для обновления количества
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0");

        Quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }

    // Расчет общей стоимости для этого товара
    public decimal GetTotalPrice() => UnitPrice * Quantity;
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

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }

    private readonly List<DomainEvent> _domainEvents = new();

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(DomainEvent eventItem)
    {
        _domainEvents.Add(eventItem);
    }

    public void RemoveDomainEvent(DomainEvent eventItem)
    {
        _domainEvents.Remove(eventItem);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

public enum OrderStatus
{
    Pending = 1,    // Заказ создан, ожидает обработки
    Confirmed = 2,  // Заказ подтвержден, товары зарезервированы
    Paid = 3,       // Оплата прошла успешно
    Shipped = 4,    // Заказ отправлен
    Delivered = 5,  // Заказ доставлен
    Cancelled = 6   // Заказ отменен
}

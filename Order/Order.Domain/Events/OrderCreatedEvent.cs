using Order.Domain.Common;

namespace Order.Domain.Events;

public class OrderCreatedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid UserId { get; }
    public decimal TotalAmount { get; }

    public OrderCreatedEvent(Guid orderId, Guid userId, decimal totalAmount)
    {
        OrderId = orderId;
        UserId = userId;
        TotalAmount = totalAmount;
    }
}

public class OrderConfirmedEvent : DomainEvent
{
    public Guid OrderId { get; }

    public OrderConfirmedEvent(Guid orderId)
    {
        OrderId = orderId;
    }
}

public class OrderCancelledEvent : DomainEvent
{
    public Guid OrderId { get; }
    public string Reason { get; }

    public OrderCancelledEvent(Guid orderId, string reason = "")
    {
        OrderId = orderId;
        Reason = reason;
    }
}
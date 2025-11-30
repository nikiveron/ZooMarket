namespace Payment.Domain.Entities;

public class PaymentEntity : BaseEntity
{
    public Guid OrderId { get; set; } // Внешний ключ на заказ
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public PaymentMethod Method { get; set; }
    public string PaymentGateway { get; set; } = string.Empty; // "Stripe", "PayPal", etc.
    public string GatewayTransactionId { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public string FailureReason { get; set; } = string.Empty;
}

public enum PaymentStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Refunded,
    Cancelled
}

public enum PaymentMethod
{
    CreditCard,
    DebitCard,
    PayPal,
    BankTransfer,
    Crypto
}

public class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

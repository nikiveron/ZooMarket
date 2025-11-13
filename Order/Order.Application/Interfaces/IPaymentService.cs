using Order.Application.DTOs;

namespace Order.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(PaymentInfo paymentInfo);
    Task<PaymentResult> RefundPaymentAsync(Guid paymentId);
}

public class PaymentInfo
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentMethod { get; set; } = string.Empty;
    public string CardToken { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
}

public class PaymentResult
{
    public bool Success { get; set; }
    public Guid PaymentId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
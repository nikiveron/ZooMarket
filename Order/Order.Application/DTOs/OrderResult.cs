namespace Order.Application.DTOs;

public class OrderResult
{
    public bool Success { get; set; }
    public Guid OrderId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public static OrderResult SuccessResult(Guid orderId) => new OrderResult
    {
        Success = true,
        OrderId = orderId
    };

    public static OrderResult Failed(string errorMessage) => new OrderResult
    {
        Success = false,
        ErrorMessage = errorMessage
    };
}
using Order.Application.Interfaces;

namespace Order.API.Services;

public class PaymentServiceStub : IPaymentService
{
    public Task<PaymentResult> ProcessPaymentAsync(PaymentInfo paymentInfo)
    {
        return Task.FromResult(new PaymentResult
        {
            Success = true,
            PaymentId = Guid.NewGuid(),
            TransactionId = Guid.NewGuid().ToString("N")
        });
    }

    public Task<PaymentResult> RefundPaymentAsync(Guid paymentId)
    {
        return Task.FromResult(new PaymentResult
        {
            Success = true,
            PaymentId = paymentId,
            TransactionId = Guid.NewGuid().ToString("N")
        });
    }
}



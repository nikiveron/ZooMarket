using Payment.Application.DTOs;
using Payment.Application.Features.Payments;

namespace Payment.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentDto> GetPaymentByIdAsync(Guid id);
    Task<PaymentDto> GetPaymentByOrderIdAsync(Guid orderId);
    Task<IEnumerable<PaymentDto>> GetPaymentsByUserIdAsync(Guid userId);
    Task<PaymentResultDto> ProcessPaymentAsync(ProcessPaymentCommand command);
    Task<PaymentResultDto> RefundPaymentAsync(RefundPaymentCommand command);
}
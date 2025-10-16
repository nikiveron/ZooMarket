using Payment.Application.DTOs;
using MediatR;

namespace Payment.Application.Features.Payments;

public record ProcessPaymentCommand(
    Guid OrderId,
    decimal Amount,
    string Currency,
    string PaymentMethod,
    string CardToken) : IRequest<PaymentResultDto>;

public record RefundPaymentCommand(
    Guid PaymentId,
    decimal? Amount = null) : IRequest<PaymentResultDto>;

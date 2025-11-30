using Order.Application.Features.Orders;
using Order.Application.Interfaces;
using Order.Domain.Entities;
using Order.Application.DTOs;
using Order.Domain.Common;

namespace Order.API.Services;

public class OrderOrchestratorService
{
    private readonly IPaymentService _paymentService;
    private readonly ICatalogService _catalogService;
    private readonly IOrderRepository _orderRepository;
    private readonly OutboxService _outboxService;
    private readonly ILogger<OrderOrchestratorService> _logger;

    public OrderOrchestratorService(
        IPaymentService paymentService,
        ICatalogService catalogService,
        IOrderRepository orderRepository,
        OutboxService outboxService,
        ILogger<OrderOrchestratorService> logger)
    {
        _paymentService = paymentService;
        _catalogService = catalogService;
        _orderRepository = orderRepository;
        _outboxService = outboxService;
        _logger = logger;
    }

    public async Task<OrderResult> ProcessOrderAsync(CreateOrderCommand command)
    {
        _logger.LogInformation("Starting order processing for user {UserId}", command.UserId);

        await using var transaction = await _orderRepository.BeginTransactionAsync();

        try
        {
            // Проверка наличия товаров
            _logger.LogInformation("Checking product availability");
            var availabilityResult = await _catalogService.CheckProductAvailabilityAsync(command.OrderItems);

            if (!availabilityResult.IsAvailable)
            {
                var errorMessage = "Products not available: " +
                    string.Join(", ", availabilityResult.UnavailableProducts.Select(p => p.ProductName));
                _logger.LogWarning("Product availability check failed: {ErrorMessage}", errorMessage);
                return OrderResult.Failed(errorMessage);
            }

            // Преобразуем CreateOrderItem в OrderItemData
            var orderItemsData = command.OrderItems.Select(item =>
                new OrderItemData(item.ProductId, item.Quantity)).ToList();

            // Создание заказа
            _logger.LogInformation("Creating order");
            var order = OrderEntity.Create(
                command.UserId,
                command.ShippingAddress,
                command.BillingAddress,
                orderItemsData);

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            // Обработка платежа
            _logger.LogInformation("Processing payment for order {OrderId}", order.Id);
            var paymentInfo = new PaymentInfo
            {
                OrderId = order.Id,
                Amount = order.TotalAmount,
                Currency = "USD",
                PaymentMethod = "CreditCard", // В реальном приложении из command
                CustomerEmail = "customer@example.com" // В реальном приложении из User service
            };

            var paymentResult = await _paymentService.ProcessPaymentAsync(paymentInfo);

            if (!paymentResult.Success)
            {
                _logger.LogWarning("Payment failed for order {OrderId}: {ErrorMessage}",
                    order.Id, paymentResult.ErrorMessage);
                return OrderResult.Failed($"Payment failed: {paymentResult.ErrorMessage}");
            }

            // Резервирование товаров
            _logger.LogInformation("Reserving products for order {OrderId}", order.Id);
            await _catalogService.ReserveProductsAsync(command.OrderItems);

            // Обновление статуса заказа
            order.UpdateStatus(OrderStatus.Confirmed);
            await _orderRepository.UpdateAsync(order);
            
            // Save event to outbox (Transaction Outbox pattern)
            await _outboxService.AddOutboxMessageAsync("OrderCreated", new
            {
                OrderId = order.Id,
                UserId = order.UserId,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt
            });
            
            await _orderRepository.SaveChangesAsync();

            await transaction.CommitAsync();

            _logger.LogInformation("Order {OrderId} processed successfully", order.Id);
            return OrderResult.SuccessResult(order.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error processing order for user {UserId}", command.UserId);
            return OrderResult.Failed($"Order processing failed: {ex.Message}");
        }
    }
}
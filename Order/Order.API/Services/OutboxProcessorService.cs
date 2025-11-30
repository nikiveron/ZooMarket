using Order.API.Services;
using Order.Infrastructure.Data;
using RabbitMQ.Client;
using System.Text;

namespace Order.API.Services;

public class OutboxProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessorService> _logger;

    public OutboxProcessorService(IServiceProvider serviceProvider, ILogger<OutboxProcessorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var outboxService = scope.ServiceProvider.GetRequiredService<OutboxService>();
                var context = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
                var rabbitMqChannel = scope.ServiceProvider.GetRequiredService<IModel>();

                var messages = await outboxService.GetUnprocessedMessagesAsync();

                foreach (var message in messages)
                {
                    try
                    {
                        var body = Encoding.UTF8.GetBytes(message.Payload);
                        rabbitMqChannel.BasicPublish(
                            exchange: "",
                            routingKey: "order_events",
                            body: body);

                        await outboxService.MarkAsProcessedAsync(message.Id);
                        _logger.LogInformation("Processed outbox message {MessageId}", message.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing outbox message {MessageId}", message.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in outbox processor");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}


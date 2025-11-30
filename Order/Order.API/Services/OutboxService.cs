using Order.Domain.Entities;
using Order.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Order.API.Services;

public class OutboxService
{
    private readonly OrderingDbContext _context;

    public OutboxService(OrderingDbContext context)
    {
        _context = context;
    }

    public async Task AddOutboxMessageAsync(string eventType, object payload)
    {
        var message = new OutboxMessage
        {
            EventType = eventType,
            Payload = System.Text.Json.JsonSerializer.Serialize(payload),
            IsProcessed = false
        };

        await _context.OutboxMessages.AddAsync(message);
    }

    public async Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize = 10)
    {
        return await _context.OutboxMessages
            .Where(m => !m.IsProcessed)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync();
    }

    public async Task MarkAsProcessedAsync(Guid messageId)
    {
        var message = await _context.OutboxMessages.FindAsync(messageId);
        if (message != null)
        {
            message.IsProcessed = true;
            message.ProcessedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}


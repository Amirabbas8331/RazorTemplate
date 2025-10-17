using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NServiceBus.Outbox;
using Quartz;
using RazorTemplate.Context;

namespace TimeMangement;
[DisallowConcurrentExecution]
public class ProcessOutboxMessagesJob : IJob
{
    private readonly ShopDbConext _dbConext;

    private readonly IPublisher _publisher;
    public ProcessOutboxMessagesJob(ShopDbConext dbConext, IPublisher publisher)
    {
        _publisher = publisher;
        _dbConext = dbConext;
    }
    public async Task Execute(IJobExecutionContext context)
    {
        List<OutboxMessage> messages = await _dbConext
            .Set<OutboxMessage>()
            .Where(m => m.MessageId == null)
            .Take(20)
            .ToListAsync(context.CancellationToken);

        foreach (OutboxMessage outboxMessage in messages)
        {
            IDomainEvent? domainEvent = JsonConvert
                .DeserializeObject<IDomainEvent>(
                    outboxMessage.MessageId.ToString(),
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });

            if (domainEvent is null)
            {
                continue;
            }

            await _publisher.Publish(domainEvent, context.CancellationToken);

            await _dbConext.SaveChangesAsync();
        }
    }
}


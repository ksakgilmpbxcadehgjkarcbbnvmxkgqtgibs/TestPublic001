namespace WorkingCore.Infrastructure.RabbitMq;

public record TaskCompletedEvent(
    Guid TaskId,
    string Title,
    DateTimeOffset CompletedAt,
    string Priority
);

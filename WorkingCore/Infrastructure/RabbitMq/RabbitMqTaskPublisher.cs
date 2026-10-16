using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace WorkingCore.Infrastructure.RabbitMq;

public class RabbitMqTaskPublisher(ILogger<RabbitMqTaskPublisher> logger, IConfiguration configuration) : BackgroundService, ITaskEventPublisher
{
    private readonly Channel<TaskCompletedEvent> _channel =
        Channel.CreateUnbounded<TaskCompletedEvent>();


    private IConnection? _connection;

    private IChannel? _rabbitChannel;

    public void Publish(TaskCompletedEvent @event)
    {
        _channel.Writer.TryWrite(@event);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitRabbitMqAsync(stoppingToken);

        await foreach (var @event in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                if (_rabbitChannel is not null)
                {
                    var json = JsonSerializer.Serialize(@event);
                    var body = Encoding.UTF8.GetBytes(json);

                    await _rabbitChannel.BasicPublishAsync(
                    exchange: "task.events",
                    routingKey: "task.completed",
                    body: body,
                    cancellationToken: stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to publish message to RabbitMQ for Task {@event.TaskId}");
            }
        }
    }
    private async Task InitRabbitMqAsync(CancellationToken ct)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:Host"] ?? "localhost"
            };
            _connection = await factory.CreateConnectionAsync(ct);
            _rabbitChannel = await _connection.CreateChannelAsync(cancellationToken: ct);

            await _rabbitChannel.ExchangeDeclareAsync(
                exchange: "task.events",
                type: ExchangeType.Direct,
                durable: true,
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RabbitMQ is unavailable on startup. Messages will be dropped or logged.");
        }
    }
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping RabbitMQ publisher, flushing queue...");

        _channel.Writer.TryComplete();

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            await base.StopAsync(linkedCts.Token);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Timeout while flushing messages to RabbitMQ.");
        }
        finally
        {
            // Корректно закрываем ресурсы
            if (_rabbitChannel is not null) await _rabbitChannel.CloseAsync();
            if (_connection is not null) await _connection.CloseAsync();
        }
    }
}

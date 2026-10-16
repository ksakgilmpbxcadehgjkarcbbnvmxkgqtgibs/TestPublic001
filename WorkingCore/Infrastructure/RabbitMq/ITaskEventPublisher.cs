namespace WorkingCore.Infrastructure.RabbitMq
{
    public interface ITaskEventPublisher
    {
        void Publish(TaskCompletedEvent @event);
    }
}

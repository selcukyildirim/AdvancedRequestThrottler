namespace RequestThrottler.Core
{
    public interface IInMemoryThrottleQueue
    {
        Task EnqueueAsync(Func<Task> action);
        Task<Func<Task>> DequeueAsync(CancellationToken cancellationToken);
        void Complete();
    }
}

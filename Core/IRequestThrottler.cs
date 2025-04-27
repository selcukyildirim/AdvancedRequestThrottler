namespace RequestThrottler.Core
{
    public interface IRequestThrottler
    {
        Task ExecuteAsync(Func<Task> action);
        Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action);
    }
}

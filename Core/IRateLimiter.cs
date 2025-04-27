namespace RequestThrottler.Core
{
    public interface IRateLimiter
    {
        Task<bool> CanExecuteAsync();
    }
}

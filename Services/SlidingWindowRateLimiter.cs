using RequestThrottler.Core;
using RequestThrottler.Core.Models;
using RequestThrottler.Utils;

public class SlidingWindowRateLimiter : IRateLimiter
{
    private readonly RateLimitRule _rule;
    private readonly ITimeProvider _timeProvider;
    private readonly object _lock = new();
    private readonly Queue<DateTime> _requestTimestamps;

    public SlidingWindowRateLimiter(RateLimitRule rule, ITimeProvider timeProvider)
    {
        _rule = rule ?? throw new ArgumentNullException(nameof(rule));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _requestTimestamps = new Queue<DateTime>();
    }

    public Task<bool> CanExecuteAsync()
    {
        lock (_lock)
        {
            var now = _timeProvider.UtcNow;

            // Eski zaman penceresindeki istekleri kaldır
            while (_requestTimestamps.Count > 0 && now - _requestTimestamps.Peek() > TimeSpan.FromSeconds(_rule.WindowInSeconds))
            {
                _requestTimestamps.Dequeue();
            }

            // Eğer izin verilen limitin altındaysa istek kabul edilir
            if (_requestTimestamps.Count < _rule.Limit)
            {
                _requestTimestamps.Enqueue(now);
                return Task.FromResult(true);
            }

            return Task.FromResult(false); // Limit aşıldı
        }
    }
}

using RequestThrottler.Core;
using RequestThrottler.Core.Models;
using RequestThrottler.Utils;

public class TokenBucketRateLimiter : IRateLimiter
{
    private readonly RateLimitRule _rule;
    private readonly ITimeProvider _timeProvider;
    private readonly object _lock = new();
    private double _tokens;
    private DateTime _lastRefill;

    public TokenBucketRateLimiter(RateLimitRule rule, ITimeProvider timeProvider)
    {
        _rule = rule ?? throw new ArgumentNullException(nameof(rule));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _tokens = rule.Limit;
        _lastRefill = _timeProvider.UtcNow;
    }

    public Task<bool> CanExecuteAsync()
    {
        lock (_lock)
        {
            var now = _timeProvider.UtcNow;
            var timeElapsed = (now - _lastRefill).TotalSeconds;

            // Tokenları doldur
            var tokensToAdd = timeElapsed * (_rule.Limit / _rule.WindowInSeconds);
            _tokens = Math.Min(_rule.Limit, _tokens + tokensToAdd);
            _lastRefill = now;

            // Eğer yeterli token varsa istek kabul edilir
            if (_tokens >= 1)
            {
                _tokens--;
                return Task.FromResult(true);
            }

            return Task.FromResult(false); // Token yok
        }
    }
}

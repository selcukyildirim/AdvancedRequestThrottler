using RequestThrottler.Core;
using RequestThrottler.Core.Models;
using RequestThrottler.Utils;

namespace RequestThrottler.Services
{
    public class FixedWindowRateLimiter : IRateLimiter
    {
        private readonly RateLimitRule _rule;
        private readonly ITimeProvider _timeProvider;
        private DateTime _windowStart;
        private int _requestCount;
        private readonly object _lock = new();

        public FixedWindowRateLimiter(RateLimitRule rule, ITimeProvider timeProvider)
        {
            _rule = rule ?? throw new ArgumentNullException(nameof(rule));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            _windowStart = _timeProvider.UtcNow;
            _requestCount = 0;
        }

        public Task<bool> CanExecuteAsync()
        {
            lock (_lock)
            {
                var now = _timeProvider.UtcNow;
                if (now - _windowStart > TimeSpan.FromSeconds(_rule.WindowInSeconds))
                {
                    _windowStart = now;
                    _requestCount = 0;
                }

                if (_requestCount < _rule.Limit)
                {
                    _requestCount++;
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
        }
    }
}

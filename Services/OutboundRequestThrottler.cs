using System;
using System.Threading.Tasks;
using RequestThrottler.Core;
using RequestThrottler.Core.Models;
using RequestThrottler.Services;

namespace RequestThrottler.Services
{
    public class ThrottleException : Exception
    {
        public ThrottleException(string message) : base(message) { }
    }

    public class OutboundRequestThrottler : IOutboundRequestThrottler
    {
        private readonly IRateLimiter _rateLimiter;
        private readonly IInMemoryThrottleQueue _queue;

        public OutboundRequestThrottler(IRateLimiter rateLimiter, IInMemoryThrottleQueue queue)
        {
            _rateLimiter = rateLimiter;
            _queue = queue;
        }

        public async Task WaitBeforeSendAsync()
        {
            if (!await _rateLimiter.CanExecuteAsync())
                throw new ThrottleException("Rate limit exceeded");

            await _queue.EnqueueAsync(() => Task.CompletedTask);
        }
    }
}

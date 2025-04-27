using RequestThrottler.Core;
using RequestThrottler.Core.Models;
using System.Threading.Channels;

namespace RequestThrottler.Services
{
    public class InMemoryThrottleQueue : IInMemoryThrottleQueue
    {
        private readonly Channel<ThrottledRequest> _channel;
        private readonly IRateLimiter _rateLimiter;

        public InMemoryThrottleQueue(IRateLimiter rateLimiter)
        {
            _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));
            _channel = Channel.CreateUnbounded<ThrottledRequest>();
        }

        public async Task EnqueueAsync(Func<Task> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            await _channel.Writer.WriteAsync(new ThrottledRequest(action));
        }

        public async Task<Func<Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            while (await _channel.Reader.WaitToReadAsync(cancellationToken))
            {
                if (_channel.Reader.TryRead(out var request))
                {
                    if (await _rateLimiter.CanExecuteAsync())
                    {
                        return request.Action;
                    }
                    else
                    {
                        await Task.Delay(50, cancellationToken);
                        await _channel.Writer.WriteAsync(request, cancellationToken);
                    }
                }
            }

            return () => Task.CompletedTask;
        }

        public void Complete()
        {
            _channel.Writer.Complete();
        }
    }
}

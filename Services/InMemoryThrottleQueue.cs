using RequestThrottler.Core;
using RequestThrottler.Core.Models;
using System.Threading.Channels;

namespace RequestThrottler.Services
{
    public class InMemoryThrottleQueue : IInMemoryThrottleQueue
    {
        private readonly Channel<ThrottledRequest> _channel;
        private readonly IRateLimiter _rateLimiter;

        public InMemoryThrottleQueue(IRateLimiter rateLimiter, int queueCapacity = 100)
        {
            _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));

            var options = new BoundedChannelOptions(queueCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait // Kuyruk dolduğunda bekleme modunda olacak
            };

            _channel = Channel.CreateBounded<ThrottledRequest>(options);
        }

        public async Task EnqueueAsync(Func<Task> action, CancellationToken cancellationToken = default)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            try
            {
                await _channel.Writer.WriteAsync(new ThrottledRequest(action), cancellationToken);
            }
            catch (ChannelClosedException)
            {
                throw new InvalidOperationException("Queue is closed and cannot accept new items.");
            }
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
